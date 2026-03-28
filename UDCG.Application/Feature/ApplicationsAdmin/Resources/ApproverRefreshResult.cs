using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDCG.Application.Feature.ApplicationsAdmin.Resources
{
    public sealed class ApproverRefreshResult
    {
        public int Considered { get; set; }
        public int Updated { get; set; }
        public int StillStuck { get; set; }

        public Dictionary<string, int> FailureCounts { get; set; } = new();
        public List<ApproverRefreshFailure> Failures { get; set; } = new();
      //  public List<ApproverRefreshPreview> Preview { get; set; } = new();
    }

    public class ApproverRefreshFailure
    {
        public int ApplicationId { get; set; }
        public string StatusText { get; set; } = "";
        public string Reason { get; set; } = "";        
        public string? DesiredOwner { get; set; }          
        public string? Detail { get; set; }
        public string ReferenceNumber { get; set; } = "";
        public string? DesiredOwnerStaffNumber { get; set; }
        public string? DesiredOwnerName { get; set; }
    }

    public class ApproverRefreshPreview
    {
        public int ApplicationId { get; set; }
        public string ReferenceNumber { get; set; } = "";
        public string StatusText { get; set; } = "";

        public string? CurrentApproverStaffNumber { get; set; }

        public string? DesiredApproverStaffNumber { get; set; }
        public string? DesiredApproverName { get; set; }

        public string Outcome { get; set; } = "OK";
    }

}
