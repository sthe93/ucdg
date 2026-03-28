using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDCG.Application.Feature.ApplicationsDashboard.Resources
{
    public static class WorkflowStatuses
    {
        public static readonly string[] HodPending =
        {
            "Pending Approval",
            "Pending Approval By HOD"
        };

        public static readonly string[] ViceDeanPending =
        {
            "Pending Approval by UCDG_VICE_DEAN",
            "Approved by UCDG_HOD"
        };

        public static readonly string[] FundAdminPending =
        {
            "Pending Approval by UCDG_Fund_Admin",
            "Approved by UCDG_VICE_DEAN"
        };

        public static readonly string[] SiaPending =
        {
            "Pending Approval by UCDG_SIA_Director",
            "Approved by UCDG_Fund_Admin"
        };

        public static readonly string[] SiaCompleted =
        {
            "Approved by UCDG_SIA_Director",
            "Award Letter Accepted",
            "Award Letter Declined"
        };

        public static readonly string[] Draft =
        {
            "Incomplete"
        };
    }
}
