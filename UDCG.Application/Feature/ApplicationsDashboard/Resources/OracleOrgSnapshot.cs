using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDCG.Application.Feature.ApplicationsDashboard.Resources
{
    public sealed class OracleOrgSnapshot
    {
        public string StaffNo { get; init; } = "";
        public string? LineManagerStaffNo { get; init; }  // HOD (supervisor)
        public string? ViceDeanStaffNo { get; init; }     // HOD's supervisor (your current "vice dean" logic)
    }
}
