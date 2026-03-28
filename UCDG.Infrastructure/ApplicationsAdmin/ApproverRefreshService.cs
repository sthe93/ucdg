using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UCDG.Persistence;
using UDCG.Application.Feature.ApplicationsAdmin.Interface;
using UDCG.Application.Feature.ApplicationsAdmin.Resources;
using UDCG.Application.Feature.ApplicationsDashboard.Interface;
using UDCG.Application.Feature.ApplicationsDashboard.Resources;
using UDCG.Application.Feature.Roles.Interfaces;

namespace UCDG.Infrastructure.ApplicationsAdmin
{
    public class ApproverRefreshService : IApproverRefreshService
    {
        private readonly UCDGDbContext _ucdg;
        private readonly UserStoreDbContext _userStore;
        private readonly IApproverResolver _resolver;

        private readonly IStaffAd _staffAd;
        public ApproverRefreshService(UCDGDbContext ucdg, UserStoreDbContext userStore, IStaffAd staffAd, IApproverResolver approverResolver)
        {
            _ucdg = ucdg;
            _userStore = userStore;
            _staffAd = staffAd;
            _resolver = approverResolver;
        }

        public async Task<ApproverRefreshResult> RefreshActiveApproverAssignmentsAsync(int actorUserStoreUserId)
        {
            var nowUtc = DateTime.UtcNow;

            // 1) Role owners (single owners) - HRPostNumber is the employee/staff number
            var fundAdminStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("Fund Administrator"))?.Trim();
            var siaDirectorStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("SIA Director"))?.Trim();

            // 2) MEC set
            var mecList = await _ucdg.MecMembers.AsNoTracking()
                .Select(m => m.EmployeeNo)
                .Where(x => x != null && x != "")
                .ToListAsync();

            var mecSet = new HashSet<string>(mecList, StringComparer.OrdinalIgnoreCase);

            // 3) Active workflow status texts
            var activeStatusTexts = new[]
            {
        "Returned for Info",
        "Pending Approval",
        "Pending Approval By HOD",
        "Pending Approval by UCDG_VICE_DEAN",
        "Pending Approval by UCDG_Fund_Admin",
        "Pending Approval by UCDG_SIA_Director",

        // DEV / legacy in-flight markers
        "Approved by UCDG_HOD",
        "Approved by UCDG_VICE_DEAN",
        "Approved by UCDG_Fund_Admin",
        "Approved by UCDG_SIA_Director"
    };

            var activeStatusIds = await _ucdg.ApplicationStatus.AsNoTracking()
                .Where(s => s.Status != null && activeStatusTexts.Contains(s.Status))
                .Select(s => s.ApplicationStatusId)
                .ToListAsync();

            if (activeStatusIds.Count == 0)
                return new ApproverRefreshResult();

            // 4) Pull active apps (minimal shape)
            var activeApps = await _ucdg.Applications.AsNoTracking()
                .Where(a => activeStatusIds.Contains(a.ApplicationStatusId))
                .Select(a => new
                {
                    a.Id,
                    a.ReferenceNumber,
                    LegacyApplicantUserId = a.UserId,
                    a.ApplicantUserStoreUserId,
                    a.ApplicationStatusId,
                    a.CurrentApproverStaffNumber
                })
                .ToListAsync();

            if (activeApps.Count == 0)
                return new ApproverRefreshResult();

            var candidates = activeApps; 

            // 5) Reference numbers for failures
            var refByAppId = candidates.ToDictionary(x => x.Id, x => x.ReferenceNumber ?? "");

            // 6) Status text map for candidates
            var usedStatusIds = candidates.Select(c => c.ApplicationStatusId).Distinct().ToList();
            var statusTextById = await _ucdg.ApplicationStatus.AsNoTracking()
                .Where(s => usedStatusIds.Contains(s.ApplicationStatusId))
                .Select(s => new { s.ApplicationStatusId, s.Status })
                .ToDictionaryAsync(x => x.ApplicationStatusId, x => x.Status ?? "");

            // 7) Legacy applicants (ONLY to get applicant staff no and fallback fields)
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

            // 8) UserStore applicants (ONLY to get staff no when legacy missing)
            var applicantUserStoreIds = candidates
                .Where(x => x.ApplicantUserStoreUserId.HasValue)
                .Select(x => x.ApplicantUserStoreUserId!.Value)
                .Distinct()
                .ToList();

            var staffNoByUserStoreUserId = applicantUserStoreIds.Count == 0
                ? new Dictionary<int, string>()
                : await _userStore.Users.AsNoTracking()
                    .Where(u => applicantUserStoreIds.Contains(u.UserId))
                    .Where(u => u.HRPostNumber != null && u.HRPostNumber.Trim() != "")
                    .Select(u => new { u.UserId, StaffNo = u.HRPostNumber! })
                    .ToDictionaryAsync(x => x.UserId, x => x.StaffNo.Trim());

            // 9) Build applicant staff numbers (must exist to query Oracle)
            var applicantStaffNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var app in candidates)
            {
                string? staffNo = null;

                if (app.ApplicantUserStoreUserId.HasValue &&
                    staffNoByUserStoreUserId.TryGetValue(app.ApplicantUserStoreUserId.Value, out var usStaff))
                {
                    staffNo = usStaff;
                }
                else if (legacyById.TryGetValue(app.LegacyApplicantUserId.Value, out var legacy))
                {
                    staffNo = legacy.StaffNumber?.Trim();
                }

                if (!string.IsNullOrWhiteSpace(staffNo))
                    applicantStaffNos.Add(staffNo);
            }

            // 10) Oracle org snapshots (PRIMARY source of HOD/VD)
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

            // 11) Oracle org for applicant HODs (needed for: "HOD reports to MEC -> Fund Admin")
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

            // 12) Resolve desired owner per app
            var desiredByAppId = new Dictionary<int, string?>();
            var stageByAppId = new Dictionary<int, AwaitingStage>();

            var allDesiredStaffNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var fundAdminOrSiaDesiredStaffNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var app in candidates)
            {
                statusTextById.TryGetValue(app.ApplicationStatusId, out var statusText);
                statusText ??= "";

                // Resolve applicant staff number (must exist)
                string? applicantStaffNo = null;

                if (app.ApplicantUserStoreUserId.HasValue &&
                    staffNoByUserStoreUserId.TryGetValue(app.ApplicantUserStoreUserId.Value, out var usStaff))
                {
                    applicantStaffNo = usStaff?.Trim();
                }

                if (string.IsNullOrWhiteSpace(applicantStaffNo) &&
                    legacyById.TryGetValue(app.LegacyApplicantUserId.Value, out var legacy))
                {
                    applicantStaffNo = legacy.StaffNumber?.Trim();
                }

                if (string.IsNullOrWhiteSpace(applicantStaffNo))
                {
                    desiredByAppId[app.Id] = null;
                    stageByAppId[app.Id] = AwaitingStage.Unknown;
                    continue;
                }

                // Legacy fallback fields (ONLY if Oracle missing)
                legacyById.TryGetValue(app.LegacyApplicantUserId.Value, out var legacyForResolver);
                legacyForResolver ??= new LegacyApplicantOrg
                {
                    UserId = app.LegacyApplicantUserId.Value,
                    StaffNumber = applicantStaffNo,
                    HodStaffNumber = null,
                    ViceDeanStaffNumber = null
                };

                // Oracle snapshots
                oracleApplicantByStaffNo.TryGetValue(applicantStaffNo, out var applicantOrg);

                OracleOrgSnapshot? hodOrg = null;
                var hodNo = applicantOrg?.LineManagerStaffNo?.Trim();
                if (!string.IsNullOrWhiteSpace(hodNo))
                    oracleHodByStaffNo.TryGetValue(hodNo, out hodOrg);

                var (desiredStaff, stage) = _resolver.Resolve(
                    statusText,
                    applicantOrg,
                    hodOrg,
                    legacyForResolver,
                    applicantStaffNo,
                    fundAdminStaffNo,
                    siaDirectorStaffNo,
                    mecSet
                );

                var desired = desiredStaff?.Trim();
                desiredByAppId[app.Id] = desired;
                stageByAppId[app.Id] = stage;

                if (!string.IsNullOrWhiteSpace(desired))
                {
                    allDesiredStaffNos.Add(desired);

                    // Validate only role-owner stages against UserStore
                    if (stage == AwaitingStage.FundAdmin || stage == AwaitingStage.SiaDirector)
                        fundAdminOrSiaDesiredStaffNos.Add(desired);
                }
            }

            // 13) Oracle names (single batch call) - nicer failure display
            var nameByStaffNo = allDesiredStaffNos.Count == 0
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : await _staffAd.GetEmployeeNamesByStaffNumbersAsync(allDesiredStaffNos);

            // 14) Validate ONLY FundAdmin/SIA desired owners exist/active/unlocked in UserStore
            Dictionary<string, (bool IsActive, bool IsLocked)> stateByStaff;

            if (fundAdminOrSiaDesiredStaffNos.Count == 0)
            {
                stateByStaff = new Dictionary<string, (bool IsActive, bool IsLocked)>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                var desiredApproverStates = await _userStore.Users.AsNoTracking()
                    .Where(u => u.HRPostNumber != null && fundAdminOrSiaDesiredStaffNos.Contains(u.HRPostNumber))
                    .Select(u => new
                    {
                        StaffNo = u.HRPostNumber!,
                        u.IsActive,
                        u.IsLocked
                    })
                    .ToListAsync();

                stateByStaff = desiredApproverStates
                    .Where(x => !string.IsNullOrWhiteSpace(x.StaffNo))
                    .ToDictionary(
                        x => x.StaffNo.Trim(),
                        x => (x.IsActive, x.IsLocked),
                        StringComparer.OrdinalIgnoreCase
                    );
            }

            // 15) Load entities for update
            var candidateIds = candidates.Select(c => c.Id).ToList();
            var appsToUpdate = await _ucdg.Applications
                .Where(a => candidateIds.Contains(a.Id))
                .ToListAsync();

            int updated = 0, stillStuck = 0;
            var failureCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var failures = new List<ApproverRefreshFailure>();

            foreach (var entity in appsToUpdate)
            {
                var referenceNumber = refByAppId.TryGetValue(entity.Id, out var rn) ? rn : "";

                statusTextById.TryGetValue(entity.ApplicationStatusId, out var statusText);
                statusText ??= "";

                var desired = desiredByAppId.TryGetValue(entity.Id, out var d) ? Norm(d) : "";
                var stage = stageByAppId.TryGetValue(entity.Id, out var stg) ? stg : AwaitingStage.Unknown;

                var desiredName =
                    (!string.IsNullOrWhiteSpace(desired) && nameByStaffNo.TryGetValue(desired, out var nm))
                        ? nm
                        : null;

                string? newOwner = null;

                if (string.IsNullOrWhiteSpace(desired))
                {
                    AddFail(failureCounts, failures, entity.Id, referenceNumber, statusText,
                        "DESIRED_OWNER_NULL", null, null,
                        "Resolver returned null/empty (missing org info, missing role owners, or status not handled).");
                }
                else
                {
                    if (stage == AwaitingStage.FirstLineManager ||
                        stage == AwaitingStage.SecondLineManager ||
                        stage == AwaitingStage.ApplicantAwardDecisionOrInfo)
                    {
                        newOwner = desired;
                    }
                    else if (stage == AwaitingStage.FundAdmin || stage == AwaitingStage.SiaDirector)
                    {
                        if (!stateByStaff.TryGetValue(desired, out var st))
                        {
                            AddFail(failureCounts, failures, entity.Id, referenceNumber, statusText,
                                "DESIRED_OWNER_NOT_IN_USERSTORE", desired, desiredName,
                                "Role owner staff no not found in UserStore.Users by HRPostNumber.");
                        }
                        else if (!st.IsActive)
                        {
                            AddFail(failureCounts, failures, entity.Id, referenceNumber, statusText,
                                "DESIRED_OWNER_INACTIVE", desired, desiredName,
                                "UserStore user exists but IsActive=false.");
                        }
                        else if (st.IsLocked)
                        {
                            AddFail(failureCounts, failures, entity.Id, referenceNumber, statusText,
                                "DESIRED_OWNER_LOCKED", desired, desiredName,
                                "UserStore user exists but IsLocked=true.");
                        }
                        else
                        {
                            newOwner = desired;
                        }
                    }
                    else
                    {
                        AddFail(failureCounts, failures, entity.Id, referenceNumber, statusText,
                            "STAGE_UNKNOWN", desired, desiredName,
                            "Resolver stage unknown/not handled.");
                    }
                }

                // IMPORTANT CHANGE:
                // We do NOT clear CurrentApprover when resolver fails (newOwner == null).
                // That would "lose" the last known owner and break oversight/inbox.
                if (!string.IsNullOrWhiteSpace(newOwner) &&
                    !string.Equals(entity.CurrentApproverStaffNumber, newOwner, StringComparison.OrdinalIgnoreCase))
                {
                    entity.CurrentApproverStaffNumber = newOwner;
                    updated++;
                }

                if (string.IsNullOrWhiteSpace(newOwner))
                    stillStuck++;

                entity.LastApproverRefreshDateUtc = nowUtc;
                entity.LastApproverRefreshByUserStoreUserId = actorUserStoreUserId;
            }

            await _ucdg.SaveChangesAsync();

            return new ApproverRefreshResult
            {
                Considered = activeApps.Count,
                Updated = updated,
                StillStuck = stillStuck,
                FailureCounts = failureCounts,
                Failures = failures
            };
        }
        private static string Norm(string? s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim();

        private static void AddFail(Dictionary<string, int> counts, List<ApproverRefreshFailure> list, int appId, string referenceNumber, string statusText, string reason,
            string? desiredOwnerStaffNumber, string? desiredOwnerName, string? detail, int max = 100)
        {
            counts[reason] = counts.TryGetValue(reason, out var c) ? c + 1 : 1;

            if (list.Count >= max) return;

            list.Add(new ApproverRefreshFailure
            {
                ApplicationId = appId,
                ReferenceNumber = referenceNumber ?? "",
                StatusText = statusText ?? "",
                Reason = reason,
                DesiredOwnerStaffNumber = desiredOwnerStaffNumber,
                DesiredOwnerName = desiredOwnerName ?? (string.IsNullOrWhiteSpace(desiredOwnerStaffNumber) ? null : "Name not found in Oracle"),
                Detail = detail
            });
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
                      && !string.IsNullOrWhiteSpace(u.HRPostNumber)
                orderby u.UserId
                select u.HRPostNumber.Trim()
            ).FirstOrDefaultAsync();

            return staffNo;
        }

        public async Task<(string? FundAdminStaffNo, string? SiaDirectorStaffNo)> GetCurrentRoleOwnersAsync()
        {
            var fundAdminStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("Fund Administrator"))?.Trim();
            var siaDirectorStaffNo = (await GetSingleOwnerStaffNumberByRoleTypeAsync("SIA Director"))?.Trim();

            return (fundAdminStaffNo, siaDirectorStaffNo);
        }

    }
}
