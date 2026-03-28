using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDCG.Application.Feature.ApplicationsDashboard.Resources
{
    public class DashboardCapsDto
    {
        public bool HasInbox { get; set; }
        public bool HasTempAssignments { get; set; }
        public bool HasTeamHistory { get; set; }
        public bool ShowApproverTabs { get; set; }
        public bool CanApply { get; set; }
    }
}
