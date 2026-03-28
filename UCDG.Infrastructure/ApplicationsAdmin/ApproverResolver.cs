using System;
using System.Collections.Generic;
using UDCG.Application.Feature.ApplicationsAdmin.Interface;
using UDCG.Application.Feature.ApplicationsAdmin.Resources;
using UDCG.Application.Feature.ApplicationsDashboard.Interface;
using UDCG.Application.Feature.ApplicationsDashboard.Resources;

namespace UCDG.Infrastructure.ApplicationsAdmin
{
    public class ApproverResolver : IApproverResolver
    {
        public ApproverResolution Resolve(string statusText, OracleOrgSnapshot? applicantOrg, OracleOrgSnapshot? hodOrg, LegacyApplicantOrg legacy,
            string applicantStaffNo, string? fundAdminStaffNo, string? siaDirectorStaffNo, HashSet<string> mecSet)
        {
            statusText ??= "";
            applicantStaffNo = (applicantStaffNo ?? "").Trim();

            if (statusText.Equals("Returned for Info", StringComparison.OrdinalIgnoreCase))
                return new ApproverResolution(applicantStaffNo, AwaitingStage.ApplicantAwardDecisionOrInfo);

            // Oracle first, legacy fallback
            var hod = (applicantOrg?.LineManagerStaffNo ?? legacy.HodStaffNumber)?.Trim();
            var viceDean = (applicantOrg?.ViceDeanStaffNo ?? legacy.ViceDeanStaffNumber)?.Trim();

            if (statusText.Equals("Pending Approval", StringComparison.OrdinalIgnoreCase) ||
                statusText.Equals("Pending Approval By HOD", StringComparison.OrdinalIgnoreCase))
            {
                //  if hod reports to MEC -> straight to Fund Admin
                if (!string.IsNullOrWhiteSpace(hod) && mecSet.Contains(hod))
                    return new ApproverResolution(fundAdminStaffNo?.Trim(), AwaitingStage.FundAdmin);

                return new ApproverResolution(hod, AwaitingStage.FirstLineManager);
            }

            if (statusText.Equals("Pending Approval by UCDG_VICE_DEAN", StringComparison.OrdinalIgnoreCase) ||
                statusText.Equals("Approved by UCDG_HOD", StringComparison.OrdinalIgnoreCase))
            {
                //  if vice dean reports to MEC -> straight to Fund Admin
                var hodReportsTo = hodOrg?.LineManagerStaffNo?.Trim(); // line's supervisor from Oracle
                if (!string.IsNullOrWhiteSpace(hodReportsTo) && mecSet.Contains(hodReportsTo))
                    return new ApproverResolution(fundAdminStaffNo?.Trim(), AwaitingStage.FundAdmin);

                return new ApproverResolution(viceDean, AwaitingStage.SecondLineManager);
            }

            if (statusText.Equals("Pending Approval by UCDG_Fund_Admin", StringComparison.OrdinalIgnoreCase) ||
                statusText.Equals("Approved by UCDG_VICE_DEAN", StringComparison.OrdinalIgnoreCase))
                return new ApproverResolution(fundAdminStaffNo?.Trim(), AwaitingStage.FundAdmin);

            if (statusText.Equals("Pending Approval by UCDG_SIA_Director", StringComparison.OrdinalIgnoreCase) ||
                statusText.Equals("Approved by UCDG_Fund_Admin", StringComparison.OrdinalIgnoreCase))
                return new ApproverResolution(siaDirectorStaffNo?.Trim(), AwaitingStage.SiaDirector);

            if (statusText.Equals("Approved by UCDG_SIA_Director", StringComparison.OrdinalIgnoreCase))
                return new ApproverResolution(applicantStaffNo, AwaitingStage.ApplicantAwardDecisionOrInfo);

            return new ApproverResolution(null, AwaitingStage.Unknown);
        }
    }
}