using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDCG.Application.Feature.ApplicationsAdmin.Resources;
using UDCG.Application.Feature.ApplicationsDashboard.Resources;

namespace UDCG.Application.Feature.ApplicationsDashboard.Interface
{
    public readonly record struct ApproverResolution(string? StaffNumber, AwaitingStage Stage);
    public interface IApproverResolver
    {
        public ApproverResolution Resolve(string statusText, OracleOrgSnapshot? applicantOrg, OracleOrgSnapshot? hodOrg, LegacyApplicantOrg legacy,
            string applicantStaffNo, string? fundAdminStaffNo, string? siaDirectorStaffNo, HashSet<string> mecSet);
    }
}
