using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UCDG.Domain.Entities;
using UCDG.Persistence;
using UCDG.Persistence.Enums;
using UDCG.Application.Feature.ApplicationsAdmin.Resources;
using UDCG.Application.Feature.ApplicationsDashboard.Interface;
using UDCG.Application.Feature.ApplicationsDashboard.Resources;
using UDCG.Application.Feature.Roles.Interfaces;

public class ApplicationsDashboardRepository : IApplicationsDashboardRepository
{
    private readonly UCDGDbContext _ucdg;
    private readonly UserStoreDbContext _userStore;
    private readonly IApproverResolver _resolver;
    private readonly IStaffAd _staffAd;


    public ApplicationsDashboardRepository(UCDGDbContext ucdg, UserStoreDbContext userStore, IApproverResolver resolver, IStaffAd staffAd)
    {
        _ucdg = ucdg;
        _userStore = userStore;
        _resolver = resolver;
        _staffAd = staffAd;
    }


    public async Task<DashboardCapsDto> GetDashboardCapsAsync(int userStoreUserId, string staffNumber, bool isFundAdmin, bool isSiaDirector)
    {
        staffNumber = (staffNumber ?? "").Trim();

        var hasTempAssignments = await _userStore.TemporaryUserRoles.AnyAsync(t => t.UserId == userStoreUserId && t.IsActive);

        // Candidate workflow statuses (MUST match GetInboxAsync candidate list / resolver coverage)
        var inboxCandidateStatusTexts = new[]
        {
            "Returned for Info",
            "Pending Approval",
            "Pending Approval By HOD",
            "Pending Approval by UCDG_VICE_DEAN",
            "Pending Approval by UCDG_Fund_Admin",
            "Pending Approval by UCDG_SIA_Director",

            "Approved by UCDG_HOD",
            "Approved by UCDG_VICE_DEAN",
            "Approved by UCDG_Fund_Admin",
            "Approved by UCDG_SIA_Director"
        };

        var excludedStatusIds = await GetExcludedManagerStatusIdsAsync();

        var inboxCandidateStatusIds = await _ucdg.ApplicationStatus.Where(s => s.Status != null && inboxCandidateStatusTexts.Contains(s.Status))
            .Select(s => s.ApplicationStatusId).ToListAsync();

        // Inbox exists if:
        // - I have temp assignments OR
        // - anything currently assigned to me (refresh-driven) OR
        // - there are active workflow items at all (fallback resolver path handled in GetInboxAsync)
        var tempServiceUp = true;
        var hasInbox = hasTempAssignments
            || await _ucdg.Applications.AnyAsync(a =>
            a.ReferenceNumber != null &&
            a.CurrentApproverStaffNumber != null &&
            a.CurrentApproverStaffNumber.Trim() == staffNumber);

        var processedActionTypes = new[] { "Approved", "Declined", "Returned", "Routed" };

        var hasTeamHistory = await _ucdg.ApplicationActions.AnyAsync(x =>
            x.ApplicantLineManagerStaffNumberAtAction == staffNumber &&
            processedActionTypes.Contains(x.ActionType));

        var hasUserStoreTeam = await _userStore.Users.AnyAsync(u =>
            u.HRPostNumber != null && u.HRPostNumber.Trim() != "" &&
            (
                (u.LineManagerStaffNumber != null && u.LineManagerStaffNumber.Trim() == staffNumber) ||
                (u.ViceDeanStaffNumber != null && u.ViceDeanStaffNumber.Trim() == staffNumber)
            ));

        var hasLegacyTeam = await _ucdg.Users.AnyAsync(u =>
            u.HODStaffNUmber == staffNumber || u.ViceDeanStaffNUmber == staffNumber);

        var hasTeam = hasTeamHistory || hasUserStoreTeam || hasLegacyTeam;

        var isApproverOnly = isFundAdmin || isSiaDirector;
        var canApply = !isApproverOnly;

        var showApproverTabs =
            isApproverOnly ||
            hasInbox ||
            hasTempAssignments;

        return new DashboardCapsDto
        {
            HasInbox = hasInbox,
            HasTempAssignments = hasTempAssignments,
            HasTeamHistory = hasTeam,
            ShowApproverTabs = showApproverTabs,
            CanApply = canApply
        };
    }

    public async Task<List<ApplicationRowDto>> GetMyApplicationsAsync(string staffNumber, int? userStoreUserId)
    {
        staffNumber = (staffNumber ?? "").Trim();

        IQueryable<Applications> qNew = _ucdg.Applications.Where(a => false);

        var legacyUserId = await _ucdg.Users.AsNoTracking()
        .Where(u => u.StaffNumber != null && u.StaffNumber.Trim() == staffNumber)
        .Select(u => (int?)u.UserId)
        .FirstOrDefaultAsync();

        // NEW world: ApplicantUserStoreUserId match
        if (userStoreUserId.HasValue)
        {
            qNew = _ucdg.Applications.Where(a =>
                a.ReferenceNumber != null &&
                a.ApplicantUserStoreUserId == userStoreUserId.Value);
        }

        // LEGACY: join Users to resolve StaffNumber
        var qLegacy =
            from a in _ucdg.Applications
            join u in _ucdg.Users on a.UserId equals u.UserId
            where a.ReferenceNumber != null
                  && a.ApplicantUserStoreUserId == null
                  && u.StaffNumber != null
                  && u.StaffNumber == staffNumber
            select a;

        var rows = await qNew
            .Union(qLegacy)
            .OrderByDescending(a => a.ReferenceNumber)
            .Select(ToRowDtoProjection())
            .ToListAsync();

        await AddProjectsAsync(rows);
        await AddDocCountsAsync(rows);
        await AddSignOffsAsync(rows, userStoreUserId, legacyUserId);
        await AddProgressReportInfoAsync(rows);
        await EnrichAwaitingAsync(rows);

        return rows;
    }

    public async Task<List<ApplicationRowDto>> GetInboxAsync(string staffNumber, int? userStoreUserId)
    {
        staffNumber = (staffNumber ?? "").Trim();

        // 0) Temp assignments for THIS logged-in user (acting approver)
        var tempAppIds = new List<int>();
        if (userStoreUserId != null)
        {
            tempAppIds = await _userStore.TemporaryUserRoles.AsNoTracking()
                .Where(t => t.IsActive && t.UserId == userStoreUserId)
                .Select(t => t.ApplicationId)
                .ToListAsync();
        }
        var tempSet = new HashSet<int>(tempAppIds);

        // 1) Candidate workflow statuses (MUST match resolver coverage)
        var inboxCandidateStatusTexts = new[]
        {
        "Returned for Info",
        "Pending Approval",
        "Pending Approval By HOD",
        "Pending Approval by UCDG_VICE_DEAN",
        "Pending Approval by UCDG_Fund_Admin",
        "Pending Approval by UCDG_SIA_Director",

        "Approved by UCDG_HOD",
        "Approved by UCDG_VICE_DEAN",
        "Approved by UCDG_Fund_Admin",
        "Approved by UCDG_SIA_Director"
    };

        var excludedStatusIds = await GetExcludedManagerStatusIdsAsync();

        var inboxCandidateStatusIds = await _ucdg.ApplicationStatus.AsNoTracking()
            .Where(s => s.Status != null && inboxCandidateStatusTexts.Contains(s.Status))
            .Select(s => s.ApplicationStatusId)
            .ToListAsync();

        if (inboxCandidateStatusIds.Count == 0 && tempAppIds.Count == 0)
            return new List<ApplicationRowDto>();

        // 2) Pull minimal candidate info (NO UserStore dependency)
        var candidates = await _ucdg.Applications.AsNoTracking()
            .Where(a =>
                a.ReferenceNumber != null
                && !excludedStatusIds.Contains(a.ApplicationStatus.ApplicationStatusId)
                && (inboxCandidateStatusIds.Contains(a.ApplicationStatus.ApplicationStatusId) || tempSet.Contains(a.Id))
            )
            .Select(a => new
            {
                a.Id,
                LegacyApplicantUserId = a.UserId,
                StatusText = a.ApplicationStatus.Status,
                a.CurrentApproverStaffNumber
            })
            .ToListAsync();

        if (candidates.Count == 0)
            return new List<ApplicationRowDto>();

        var candidateIds = candidates.Select(c => c.Id).ToList();

        // 3) Temp assignments for ALL candidate apps (so owners can SEE "temporary approver assigned")
        var tempAssignments = await _userStore.TemporaryUserRoles.AsNoTracking()
            .Where(t => t.IsActive && candidateIds.Contains(t.ApplicationId))
            .Select(t => new { t.ApplicationId, t.UserId })
            .ToListAsync();

        var tempUserIds = tempAssignments.Select(x => x.UserId).Distinct().ToList();

        var staffNoByTempUserId = tempUserIds.Count == 0
            ? new Dictionary<int, string>()
            : await _userStore.Users.AsNoTracking()
                .Where(u => tempUserIds.Contains(u.UserId))
                .Where(u => u.HRPostNumber != null && u.HRPostNumber.Trim() != "")
                .Select(u => new { u.UserId, StaffNo = u.HRPostNumber! })
                .ToDictionaryAsync(x => x.UserId, x => x.StaffNo.Trim());

        // appId -> list of temp approver staffNos (usually 1, but supports many)
        var tempStaffNosByAppId = tempAssignments
            .GroupBy(x => x.ApplicationId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => staffNoByTempUserId.TryGetValue(x.UserId, out var s) ? s : null)
                      .Where(s => !string.IsNullOrWhiteSpace(s))
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .ToList()
            );

        // 4) Shared inputs (resolver)
        var fundAdminStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("Fund Administrator"))?.Trim();
        var siaDirectorStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("SIA Director"))?.Trim();

        var mecList = await _ucdg.MecMembers.AsNoTracking()
            .Select(m => m.EmployeeNo)
            .Where(x => x != null && x != "")
            .ToListAsync();

        var mecSet = new HashSet<string>(mecList, StringComparer.OrdinalIgnoreCase);

        // 5) Legacy applicant org for candidates (ONLY to get applicant staff number + fallback fields)
        var legacyApplicantIds = candidates.Select(c => c.LegacyApplicantUserId).Distinct().ToList();

        var legacyApplicants = legacyApplicantIds.Count == 0
            ? new List<LegacyApplicantOrg>()
            : await _ucdg.Users.AsNoTracking()
                .Where(u => legacyApplicantIds.Contains(u.UserId))
                .Select(u => new LegacyApplicantOrg
                {
                    UserId = u.UserId,
                    StaffNumber = u.StaffNumber,
                    HodStaffNumber = u.HODStaffNUmber,
                    ViceDeanStaffNumber = u.ViceDeanStaffNUmber
                })
                .ToListAsync();

        var legacyById = legacyApplicants.ToDictionary(x => x.UserId);

        // 6) Build applicant staff numbers for Oracle org lookup
        var applicantStaffNos = legacyApplicants
            .Select(x => x.StaffNumber?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 7) Oracle org snapshots (PRIMARY)
        Dictionary<string, OracleOrgSnapshot> oracleApplicantByStaffNo;
        try
        {
            oracleApplicantByStaffNo = applicantStaffNos.Count == 0
                ? new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase)
                : await _staffAd.GetOrgHierarchyByStaffNumbersAsync(applicantStaffNos);
        }
        catch
        {
            oracleApplicantByStaffNo = new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase);
        }

        // 8) Oracle org for HODs (needed for: "HOD reports to MEC -> Fund Admin")
        var hodStaffNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var snap in oracleApplicantByStaffNo.Values)
        {
            var hod = snap.LineManagerStaffNo?.Trim();
            if (!string.IsNullOrWhiteSpace(hod))
                hodStaffNos.Add(hod);
        }

        Dictionary<string, OracleOrgSnapshot> oracleHodByStaffNo;
        try
        {
            oracleHodByStaffNo = hodStaffNos.Count == 0
                ? new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase)
                : await _staffAd.GetOrgHierarchyByStaffNumbersAsync(hodStaffNos);
        }
        catch
        {
            oracleHodByStaffNo = new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase);
        }

        // 9) Compute inbox app ids for THIS staff member
        // IMPORTANT: temp apps are included, BUT we DO NOT "continue" because owner must still see it for oversight.
        var attentionIds = new HashSet<int>();

        foreach (var c in candidates)
        {
            // Acting approver always sees it
            if (tempSet.Contains(c.Id))
                attentionIds.Add(c.Id);

            // If refresh already assigned an owner, trust it (fast path)
            var currentOwner = (c.CurrentApproverStaffNumber ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(currentOwner))
            {
                if (currentOwner.Equals(staffNumber, StringComparison.OrdinalIgnoreCase))
                    attentionIds.Add(c.Id);

                continue;
            }

            // Else compute via resolver (Oracle-first + legacy fallback)
            if (!legacyById.TryGetValue(c.LegacyApplicantUserId.Value, out var legacy))
                continue;

            var applicantStaffNo = legacy.StaffNumber?.Trim();
            if (string.IsNullOrWhiteSpace(applicantStaffNo))
                continue;

            oracleApplicantByStaffNo.TryGetValue(applicantStaffNo, out var applicantOrg);

            OracleOrgSnapshot? hodOrg = null;
            var hodNo = applicantOrg?.LineManagerStaffNo?.Trim();
            if (!string.IsNullOrWhiteSpace(hodNo))
                oracleHodByStaffNo.TryGetValue(hodNo, out hodOrg);

            var res = _resolver.Resolve(
                c.StatusText ?? "",
                applicantOrg,
                hodOrg,
                legacy,
                applicantStaffNo,
                fundAdminStaffNo,
                siaDirectorStaffNo,
                mecSet
            );

            var effectiveOwner = res.StaffNumber?.Trim();

            if (!string.IsNullOrWhiteSpace(effectiveOwner) &&
                effectiveOwner.Equals(staffNumber, StringComparison.OrdinalIgnoreCase))
            {
                attentionIds.Add(c.Id);
            }
        }

        if (attentionIds.Count == 0)
            return new List<ApplicationRowDto>();

        // 10) Fetch final rows + enrich
        var rows = await _ucdg.Applications.AsNoTracking()
            .Where(a => attentionIds.Contains(a.Id))
            .OrderBy(a => a.ApplicationEndDate)
            .Select(ToRowDtoProjection())
            .ToListAsync();

        await AddProjectsAsync(rows);

        // 11) Enrich rows with temporary approver display (so users understand why it's in inbox)
        var tempNamesNeeded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in rows)
        {
            if (tempStaffNosByAppId.TryGetValue(r.Id, out var tempStaffNos) && tempStaffNos.Count > 0)
            {
                r.HasTemporaryApprover = true;
                r.IsTemporaryApproverForThis = tempSet.Contains(r.Id);

                // if you only allow one temp approver, take first
                r.TemporaryApproverStaffNumber = tempStaffNos[0];

                tempNamesNeeded.Add(tempStaffNos[0]);
            }
        }

        var tempServiceUp = true;
        Dictionary<string, string> tempNameByStaffNo;
        try
        {
            tempNameByStaffNo = tempNamesNeeded.Count == 0
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : await _staffAd.GetEmployeeNamesByStaffNumbersAsync(tempNamesNeeded);
        }
        catch
        {
            tempServiceUp = false;
            tempNameByStaffNo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (var r in rows)
        {
            if (!r.HasTemporaryApprover) continue;

            var s = r.TemporaryApproverStaffNumber?.Trim();
            if (!string.IsNullOrWhiteSpace(s) && tempNameByStaffNo.TryGetValue(s, out var nm))
                r.TemporaryApproverName = nm;

            if (r.IsTemporaryApproverForThis)
            {
                r.TemporaryApproverDisplay = "You are the temporary approver";
            }
            else
            {
                if (!tempServiceUp || string.IsNullOrWhiteSpace(r.TemporaryApproverName))
                {
                    // don’t show staff number
                    r.TemporaryApproverStaffNumber = null; // optional
                    r.TemporaryApproverDisplay = "Temporary approver assigned";
                }
                else
                {
                    r.TemporaryApproverDisplay = $"Temporary approver assigned: {r.TemporaryApproverName}";
                }
            }
        }
        var legacyUserId = await _ucdg.Users.AsNoTracking()
       .Where(u => u.StaffNumber != null && u.StaffNumber.Trim() == staffNumber)
       .Select(u => (int?)u.UserId)
       .FirstOrDefaultAsync();

        await AddDocCountsAsync(rows);
        await AddSignOffsAsync(rows, userStoreUserId, legacyUserId);
        await AddProgressReportInfoAsync(rows);
        await EnrichAwaitingAsync(rows);

        return rows;
    }



    public async Task<List<ApplicationRowDto>> GetMyTeamAsync(string staffNumber, bool canViewAllApplications, bool isSiaDirector)
    {
        staffNumber = (staffNumber ?? "").Trim();

        // 0) Status text -> id map (env safe, no hard-coded IDs)
        var statusPairs = await _ucdg.ApplicationStatus.AsNoTracking()
            .Where(s => s.Status != null && s.Status.Trim() != "")
            .Select(s => new { Text = s.Status!.Trim(), s.ApplicationStatusId })
            .ToListAsync();

        var statusIdByText = statusPairs.ToDictionary(x => x.Text, x => x.ApplicationStatusId, StringComparer.OrdinalIgnoreCase);

        HashSet<int> Map(params string[] statuses)
        {
            var set = new HashSet<int>();
            foreach (var st in statuses)
            {
                if (statusIdByText.TryGetValue(st, out var id))
                    set.Add(id);
            }
            return set;
        }

        // Drafts
        var excludedStatusIds = Map("Incomplete");

        // “Needs action” (your UI can hide these when showing History)
        var needsActionStatusIds = Map(
            "Pending Approval",
            "Pending Approval By HOD",
            "Pending Approval by UCDG_VICE_DEAN",
            "Pending Approval by UCDG_Fund_Admin",
            "Pending Approval by UCDG_SIA_Director",
            "Returned for Info" // optional: include/exclude depending on your meaning of "needs action"
        );

        // SIA lane / outcomes (SIA wants both current + historical touched)
        var siaLaneStatusIds = Map(
            "Pending Approval by UCDG_SIA_Director",
            "Approved by UCDG_Fund_Admin",
            "Approved by UCDG_SIA_Director",
            "Award Letter Accepted",
            "Award Letter Declined"
        );

        // 1) Fund Admin "all applications"
        if (canViewAllApplications)
        {
            var all = await _ucdg.Applications.AsNoTracking()
                .Where(a =>
                    a.ReferenceNumber != null &&
                    !excludedStatusIds.Contains(a.ApplicationStatusId))
                .OrderByDescending(a => a.ReferenceNumber)
                .Select(ToRowDtoProjection())
                .ToListAsync();

            // Flags not meaningful for FA "all"
            foreach (var r in all)
            {
                r.IsInMyCurrentTeam = false;
                r.IsActionedByMe = false;
                r.IsInSiaLaneHistory = false;
            }

            await AddProjectsAsync(all);
            await EnrichAwaitingAsync(all);
            return all;
        }

        // 2) Actioned-by-me app ids (old HOD keeps these forever)
        var processedActionTypes = new[] { "Approved", "Declined", "Returned", "Routed" };

        var actionedByMeIds = await _ucdg.ApplicationActions.AsNoTracking()
            .Where(x =>
                processedActionTypes.Contains(x.ActionType) &&
                (
                    (x.ActorStaffNumber != null && x.ActorStaffNumber.Trim() == staffNumber) ||
                    (x.ActingForStaffNumber != null && x.ActingForStaffNumber.Trim() == staffNumber) ||
                    (x.ApplicantLineManagerStaffNumberAtAction != null && x.ApplicantLineManagerStaffNumberAtAction.Trim() == staffNumber)
                ))
            .Select(x => x.ApplicationId)
            .Distinct()
            .ToListAsync();

        var actionedByMeSet = actionedByMeIds.ToHashSet();

        // 3) Current team app ids (new HOD sees their current team)
        var (teamUserStoreIds, teamStaffSet) = await GetMyTeamKeysAsync(staffNumber);

        IQueryable<Applications> qTeamNew = _ucdg.Applications.AsNoTracking().Where(a => false);
        if (teamUserStoreIds.Count > 0)
        {
            qTeamNew = _ucdg.Applications.AsNoTracking()
                .Where(a =>
                    a.ReferenceNumber != null &&
                    a.ApplicantUserStoreUserId != null &&
                    teamUserStoreIds.Contains(a.ApplicantUserStoreUserId.Value));
        }

        var qTeamLegacy =
            from a in _ucdg.Applications.AsNoTracking()
            join u in _ucdg.Users.AsNoTracking() on a.UserId equals u.UserId
            where a.ReferenceNumber != null
                  && a.ApplicantUserStoreUserId == null
                  && u.StaffNumber != null
                  && teamStaffSet.Contains(u.StaffNumber.Trim())
            select a;

        var qCurrentTeamApps = qTeamNew.Union(qTeamLegacy);

        var currentTeamIds = await qCurrentTeamApps
            .Where(a => !excludedStatusIds.Contains(a.ApplicationStatusId))
            .Select(a => a.Id)
            .Distinct()
            .ToListAsync();

        var currentTeamIdSet = currentTeamIds.ToHashSet();

        // 4) SIA historical “touched lane” app ids (previous directors too)
        HashSet<int> siaTouchedSet = new HashSet<int>();
        HashSet<int> siaCurrentStatusSet = new HashSet<int>();

        if (isSiaDirector)
        {
            // a) Anything currently in SIA lane/outcomes by status
            if (siaLaneStatusIds.Count > 0)
            {
                var ids = await _ucdg.Applications.AsNoTracking()
                    .Where(a =>
                        a.ReferenceNumber != null &&
                        !excludedStatusIds.Contains(a.ApplicationStatusId) &&
                        siaLaneStatusIds.Contains(a.ApplicationStatusId))
                    .Select(a => a.Id)
                    .Distinct()
                    .ToListAsync();

                siaCurrentStatusSet = ids.ToHashSet();
            }

            // b) Anything that ever transitioned through SIA statuses
            if (siaLaneStatusIds.Count > 0)
            {
                var touched = await _ucdg.ApplicationActions.AsNoTracking()
                    .Where(act =>
                        (act.FromStatusId != null && siaLaneStatusIds.Contains(act.FromStatusId.Value)) ||
                        (act.ToStatusId != null && siaLaneStatusIds.Contains(act.ToStatusId.Value))
                    )
                    .Select(act => act.ApplicationId)
                    .Distinct()
                    .ToListAsync();

                siaTouchedSet = touched.ToHashSet();
            }
        }

        // 5) SUPerset ids (single fetch)
        var supersetIds = new HashSet<int>();

        foreach (var id in currentTeamIdSet) supersetIds.Add(id);
        foreach (var id in actionedByMeSet) supersetIds.Add(id);
        if (isSiaDirector)
        {
            foreach (var id in siaCurrentStatusSet) supersetIds.Add(id);
            foreach (var id in siaTouchedSet) supersetIds.Add(id);
        }

        if (supersetIds.Count == 0)
            return new List<ApplicationRowDto>();

        // 6) Fetch rows once
        var rows = await _ucdg.Applications.AsNoTracking()
            .Where(a => supersetIds.Contains(a.Id))
            .Where(a => a.ReferenceNumber != null)
            .Where(a => !excludedStatusIds.Contains(a.ApplicationStatusId))
            .OrderByDescending(a => a.ReferenceNumber)
            .Select(ToRowDtoProjection())
            .ToListAsync();

        // 7) Enrich flags (frontend dropdown uses these)
        foreach (var r in rows)
        {
            r.IsInMyCurrentTeam = currentTeamIdSet.Contains(r.Id);
            r.IsActionedByMe = actionedByMeSet.Contains(r.Id);

            // SIA history flag = either currently in lane/outcome OR ever touched lane/outcome
            r.IsInSiaLaneHistory = isSiaDirector && (siaCurrentStatusSet.Contains(r.Id) || siaTouchedSet.Contains(r.Id));
        }

        await AddProjectsAsync(rows);
        await EnrichAwaitingAsync(rows);

        return rows;
    }



    private async Task<(List<int> TeamUserStoreIds, HashSet<string> TeamStaffNos)> GetMyTeamKeysAsync(string staffNumber)
    {
        var mgr = (staffNumber ?? "").Trim();

        // 1) UserStore team (best source when rows exist)
        var usTeam = await _userStore.Users
            .Where(u =>
                u.HRPostNumber != null && u.HRPostNumber.Trim() != "" &&
                (
                    (u.LineManagerStaffNumber != null && u.LineManagerStaffNumber.Trim() == mgr) ||
                    (u.ViceDeanStaffNumber != null && u.ViceDeanStaffNumber.Trim() == mgr)
                ))
            .Select(u => new { u.UserId, StaffNo = u.HRPostNumber })
            .ToListAsync();

        var teamUserStoreIds = usTeam.Select(x => x.UserId).Distinct().ToList();

        var teamStaffNos = new HashSet<string>(
            usTeam.Select(x => x.StaffNo!.Trim()).Where(x => x != ""),
            StringComparer.OrdinalIgnoreCase
        );

        // 2) Legacy fallback team (covers people not yet in UserStore)
        // NOTE: legacy org might be stale, but it’s better than "missing people completely".
        var legacyTeamStaffNos = await _ucdg.Users
            .Where(u =>
                u.StaffNumber != null && u.StaffNumber.Trim() != "" &&
                (u.HODStaffNUmber == mgr || u.ViceDeanStaffNUmber == mgr))
            .Select(u => u.StaffNumber)
            .ToListAsync();

        foreach (var s in legacyTeamStaffNos)
        {
            var sn = s?.Trim();
            if (!string.IsNullOrWhiteSpace(sn))
                teamStaffNos.Add(sn);
        }

        return (teamUserStoreIds, teamStaffNos);
    }

    private async Task<List<int>> GetExcludedManagerStatusIdsAsync()
    {
        var excludedTexts = new[]
        {
            "Incomplete",
            "NULL" // only if you literally have Status="NULL" rows used by apps
        };

        return await _ucdg.ApplicationStatus
            .Where(s => s.Status != null && excludedTexts.Contains(s.Status))
            .Select(s => s.ApplicationStatusId)
            .ToListAsync();
    }
    private async Task<string> GetSingleOwnerStaffNumberByRoleTypeAsync(string roleType)
    {
        var staffNo = await (
            from ur in _userStore.UserRoles.AsNoTracking()
            join r in _userStore.Roles.AsNoTracking() on ur.RoleId equals r.RoleId
            join u in _userStore.Users.AsNoTracking() on ur.UserId equals u.UserId
            where ur.IsActive
                  && r.IsActive
                  && u.IsActive
                  && !u.IsLocked
                  && !(r.IsTemporary ?? false)
                  && r.RoleType == roleType
                  && u.HRPostNumber != null && u.HRPostNumber != ""
            orderby u.UserId
            select u.HRPostNumber
        ).FirstOrDefaultAsync();

        return staffNo?.Trim();
    }
    private async Task<List<int>> GetNeedsActionStatusIdsAsync()
    {
        // These statuses represent "there is a next approver who must still act"
        var texts = new[]
        {
        "Pending Approval",
        "Pending Approval By HOD",
        "Pending Approval by UCDG_VICE_DEAN",
        "Pending Approval by UCDG_Fund_Admin",
        "Pending Approval by UCDG_SIA_Director",

        // in-flight / bridge markers (next stage still pending)
        "Approved by UCDG_HOD",        // -> Vice Dean must approve
        "Approved by UCDG_VICE_DEAN",  // -> Fund Admin must approve
        "Approved by UCDG_Fund_Admin"  // -> SIA Director must approve
    };

        return await _ucdg.ApplicationStatus
            .Where(s => s.Status != null && texts.Contains(s.Status))
            .Select(s => s.ApplicationStatusId)
            .ToListAsync();
    }

    private static Expression<Func<Applications, ApplicationRowDto>> ToRowDtoProjection() =>
    a => new ApplicationRowDto
    {
        Id = a.Id,
        ReferenceNumber = a.ReferenceNumber,
        FundingCallName = a.FundingCalls.FundingCallName,
        FundingCallId = a.FundingCallsId,
        FundingEndDate = a.FundingEndDate,
        DHETRequestedAmount = a.DHETFundsRequested,
        DHETApprovedAmount = a.ApprovedAmount,
        SubmittedBy = a.Username,
        SubmittedDate = a.SubmittedDate,
        StatusId = a.ApplicationStatus.ApplicationStatusId,
        StatusText = a.ApplicationStatus.Status,
        EndDate = a.ApplicationEndDate,
        CurrentApproverStaffNumber = a.CurrentApproverStaffNumber,
        ApplicantUserStoreUserId = a.ApplicantUserStoreUserId,
        LegacyApplicantUserId = a.UserId,
        UserId = a.ApplicantUserStoreUserId ?? a.UserId,
        Project = ""

    };
    private async Task AddProjectsAsync(List<ApplicationRowDto> rows)
    {
        if (rows == null || rows.Count == 0) return;

        var appIds = rows.Select(r => r.Id).Distinct().ToList();
        if (appIds.Count == 0) return;

        var projectPairs = await (
            from ap in _ucdg.ApplicationsProjects.AsNoTracking()
            join p in _ucdg.Projects.AsNoTracking() on ap.ProjectsId equals p.Id
            where appIds.Contains(ap.ApplicationsId)
            select new { ap.ApplicationsId, ProjectName = p.ProjectName }
        ).ToListAsync();

        var projMap = projectPairs
            .Where(x => !string.IsNullOrWhiteSpace(x.ProjectName))
            .GroupBy(x => x.ApplicationsId)
            .ToDictionary(
                g => g.Key,
                g => string.Join("<br/>",
                    g.Select(x => x.ProjectName!.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(n => n))
            );

        foreach (var r in rows)
            r.Project = projMap.TryGetValue(r.Id, out var html) ? html : "";
    }

    private async Task EnrichAwaitingAsync(List<ApplicationRowDto> rows)
    {
        if (rows == null || rows.Count == 0) return;
        // 0) Role owners + MEC set (shared inputs)
        var fundAdminStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("Fund Administrator"))?.Trim();
        var siaDirectorStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("SIA Director"))?.Trim();

        var mecList = await _ucdg.MecMembers.AsNoTracking()
            .Select(m => m.EmployeeNo)
            .Where(x => x != null && x != "")
            .ToListAsync();

        var mecSet = new HashSet<string>(mecList, StringComparer.OrdinalIgnoreCase);

        // 1) Load legacy applicant org (ONLY to get applicant staff no when UserStore missing)
        var legacyIds = rows
            .Where(r => r.LegacyApplicantUserId.HasValue)
            .Select(r => r.LegacyApplicantUserId!.Value)
            .Distinct()
            .ToList();

        var legacyApplicants = legacyIds.Count == 0
            ? new List<LegacyApplicantOrg>()
            : await _ucdg.Users.AsNoTracking()
                .Where(u => legacyIds.Contains(u.UserId))
                .Select(u => new LegacyApplicantOrg
                {
                    UserId = u.UserId,
                    StaffNumber = u.StaffNumber,
                    HodStaffNumber = u.HODStaffNUmber,
                    ViceDeanStaffNumber = u.ViceDeanStaffNUmber
                })
                .ToListAsync();

        var legacyById = legacyApplicants.ToDictionary(x => x.UserId);

        // 2) Build applicant staff numbers (from legacy rows; if you have ApplicantUserStoreUserId on rows, include that too)
        var applicantStaffNos = legacyApplicants
            .Select(x => x.StaffNumber?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 3) Oracle org snapshots (PRIMARY source of LM/VD)
        Dictionary<string, OracleOrgSnapshot> oracleApplicantByStaffNo;
        try
        {
            oracleApplicantByStaffNo = applicantStaffNos.Count == 0
                ? new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase)
                : await _staffAd.GetOrgHierarchyByStaffNumbersAsync(applicantStaffNos);
        }
        catch
        {
            oracleApplicantByStaffNo = new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase);
        }

        // 4) Oracle org for HODs (to apply rule: "HOD reports to MEC -> Fund Admin" at VD stage)
        var hodStaffNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var snap in oracleApplicantByStaffNo.Values)
        {
            var hod = snap.LineManagerStaffNo?.Trim();
            if (!string.IsNullOrWhiteSpace(hod))
                hodStaffNos.Add(hod);
        }

        Dictionary<string, OracleOrgSnapshot> oracleHodByStaffNo;
        try
        {
            oracleHodByStaffNo = hodStaffNos.Count == 0
                ? new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase)
                : await _staffAd.GetOrgHierarchyByStaffNumbersAsync(hodStaffNos);
        }
        catch
        {
            oracleHodByStaffNo = new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase);
        }

        // 5) Status text by app id (prefer row value)
        var statusByAppId = rows.ToDictionary(r => r.Id, r => r.StatusText ?? "");

        // 6) Resolve awaiting per row
        var awaitingByAppId = new Dictionary<int, ApproverResolution>();
        var staffNosToName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in rows)
        {
            // Applicant staff no (for UI/email)
            string? applicantStaffNo = null;

            if (r.LegacyApplicantUserId.HasValue &&
                legacyById.TryGetValue(r.LegacyApplicantUserId.Value, out var legacyForApplicant))
            {
                applicantStaffNo = legacyForApplicant.StaffNumber?.Trim();
                r.ApplicantStaffNumber = applicantStaffNo;
            }

            var statusText = statusByAppId.TryGetValue(r.Id, out var st) ? st : "";

            // Current assigned owner exists -> keep staff number, but recompute stage label
            var current = (r.CurrentApproverStaffNumber ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(current))
            {
                if (!string.IsNullOrWhiteSpace(applicantStaffNo) &&
                    r.LegacyApplicantUserId.HasValue &&
                    legacyById.TryGetValue(r.LegacyApplicantUserId.Value, out var legacyForResolver))
                {
                    oracleApplicantByStaffNo.TryGetValue(applicantStaffNo, out var applicantOrg);

                    OracleOrgSnapshot? hodOrg = null;
                    var hodNo = applicantOrg?.LineManagerStaffNo?.Trim();
                    if (!string.IsNullOrWhiteSpace(hodNo))
                        oracleHodByStaffNo.TryGetValue(hodNo, out hodOrg);

                    var resolved = _resolver.Resolve(
                        statusText,
                        applicantOrg,
                        hodOrg,
                        legacyForResolver,
                        applicantStaffNo,
                        fundAdminStaffNo,
                        siaDirectorStaffNo,
                        mecSet
                    );

                    awaitingByAppId[r.Id] = new ApproverResolution(current, resolved.Stage);
                    staffNosToName.Add(current);
                    continue;
                }

                // Fallback: stage unknown but show person
                awaitingByAppId[r.Id] = new ApproverResolution(current, AwaitingStage.Unknown);
                staffNosToName.Add(current);
                continue;
            }

            // No assigned owner -> compute fully via resolver
            if (string.IsNullOrWhiteSpace(applicantStaffNo)) continue;
            if (!r.LegacyApplicantUserId.HasValue) continue;
            if (!legacyById.TryGetValue(r.LegacyApplicantUserId.Value, out var legacy2)) continue;

            oracleApplicantByStaffNo.TryGetValue(applicantStaffNo, out var applicantOrg2);

            OracleOrgSnapshot? hodOrg2 = null;
            var hodNo2 = applicantOrg2?.LineManagerStaffNo?.Trim();
            if (!string.IsNullOrWhiteSpace(hodNo2))
                oracleHodByStaffNo.TryGetValue(hodNo2, out hodOrg2);

            var res = _resolver.Resolve(
                statusText,
                applicantOrg2,
                hodOrg2,
                legacy2,
                applicantStaffNo,
                fundAdminStaffNo,
                siaDirectorStaffNo,
                mecSet
            );

            awaitingByAppId[r.Id] = res;

            if (!string.IsNullOrWhiteSpace(res.StaffNumber))
                staffNosToName.Add(res.StaffNumber.Trim());
        }

        // 7) One Oracle call for names
        var staffServiceUp = true;

        Dictionary<string, string> nameByStaffNo;
        try
        {
            nameByStaffNo = staffNosToName.Count == 0
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : await _staffAd.GetEmployeeNamesByStaffNumbersAsync(staffNosToName);
        }
        catch
        {
            staffServiceUp = false;
            nameByStaffNo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        // 8) Apply awaiting fields
        foreach (var r in rows)
        {
            if (!awaitingByAppId.TryGetValue(r.Id, out var res)) continue;

            var statusText = (r.StatusText ?? "").Trim();

            if (res.Stage == AwaitingStage.ApplicantAwardDecisionOrInfo)
            {
                r.AwaitingStage = res.Stage.ToString();
                r.AwaitingStaffNumber = null;
                r.AwaitingName = null;

                if (statusText.Equals("Returned for Info", StringComparison.OrdinalIgnoreCase))
                    r.AwaitingDisplay = "Awaiting applicant (Returned for info)";
                else if (statusText.Equals("Approved by UCDG_SIA_Director", StringComparison.OrdinalIgnoreCase))
                    r.AwaitingDisplay = "Awaiting applicant (Award letter decision)";

                continue;
            }

            // --- existing logic continues below ---
            var staffNo = res.StaffNumber?.Trim();
            if (string.IsNullOrWhiteSpace(staffNo)) continue;

            r.AwaitingStaffNumber = staffNo;
            r.AwaitingStage = res.Stage.ToString();
            r.AwaitingName = nameByStaffNo.TryGetValue(staffNo, out var nm) ? nm : null;

            var stageLabel = StageLabel(res.Stage);

            var hasName = !string.IsNullOrWhiteSpace(r.AwaitingName);
            var hideStaffNo = !staffServiceUp || !hasName;

            if (hideStaffNo)
            {
                r.AwaitingStaffNumber = null;
                r.AwaitingDisplay = string.IsNullOrWhiteSpace(stageLabel)
                    ? "Awaiting approver"
                    : $"Awaiting {stageLabel}";
            }
            else
            {
                r.AwaitingDisplay = string.IsNullOrWhiteSpace(stageLabel)
                    ? $"Awaiting {r.AwaitingName}"
                    : $"Awaiting {r.AwaitingName} ({stageLabel})";
            }
        }
    }

    private static string StageLabel(AwaitingStage stage) => stage switch
    {
        AwaitingStage.FirstLineManager => "First line manager",
        AwaitingStage.SecondLineManager => "Second line manager",
        AwaitingStage.FundAdmin => "Fund Administrator",
        AwaitingStage.SiaDirector => "SIA Director",
        AwaitingStage.ApplicantAwardDecisionOrInfo => "Applicant",
        _ => ""
    };

    private async Task AddDocCountsAsync(List<ApplicationRowDto> rows)
    {
        if (rows == null || rows.Count == 0) return;

        var appIds = rows.Select(r => r.Id).Distinct().ToList();

        var counts = await _ucdg.Documents.AsNoTracking()
            .Where(d => appIds.Contains(d.ApplicationsId))
            .GroupBy(d => d.ApplicationsId)
            .Select(g => new { AppId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.AppId, x => x.Cnt);

        foreach (var r in rows)
            r.NumberOfDocuments = counts.TryGetValue(r.Id, out var c) ? c : 0;
    }

    private async Task AddSignOffsAsync(List<ApplicationRowDto> rows, int? userStoreUserId, int? legacyUserId)
    {
        if (rows == null || rows.Count == 0) return;

        var appIds = rows.Select(r => r.Id).Distinct().ToList();

        var query = _ucdg.DocumentSignOffs.AsNoTracking()
            .Where(x => appIds.Contains(x.ApplicationsId));

        // ✅ Union/OR semantics: match either new user OR legacy user
        if (userStoreUserId.HasValue && legacyUserId.HasValue)
        {
            query = query.Where(x =>
                x.UserId == legacyUserId.Value
                || x.UserStoreUserId == userStoreUserId.Value
            );
        }
        else if (legacyUserId.HasValue)
        {
            query = query.Where(x => x.UserId == legacyUserId.Value);
        }
        else if (userStoreUserId.HasValue)
        {
            query = query.Where(x => x.UserStoreUserId == userStoreUserId.Value);
        }
        else
        {
            return;
        }

        var signedSet = (await query
            .Select(x => x.ApplicationsId)
            .Distinct()
            .ToListAsync())
            .ToHashSet();

        foreach (var r in rows)
            r.IsAcknowledge = signedSet.Contains(r.Id);
    }

    private async Task AddProgressReportInfoAsync(List<ApplicationRowDto> rows)
    {
        if (rows == null || rows.Count == 0) return;

        var appIds = rows.Select(r => r.Id).Distinct().ToList();

        var reports = await _ucdg.ProgressReports.AsNoTracking()
       .Where(p => appIds.Contains(p.ApplicationId))
       .Select(p => new { ApplicationId = p.ApplicationId, ReportId = p.Id, IsComplete = p.IsComplete })
       .ToListAsync();

        var byApp = reports
            .GroupBy(x => x.ApplicationId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.ReportId).First());

        foreach (var r in rows)
        {
            if (byApp.TryGetValue(r.Id, out var rep))
            {
                r.ReportId = rep.ReportId;
                r.ProgressReportComplete = rep.IsComplete;
            }
            else
            {
                r.ReportId = null;
                r.ProgressReportComplete = false;
            }
        }
    }

}
