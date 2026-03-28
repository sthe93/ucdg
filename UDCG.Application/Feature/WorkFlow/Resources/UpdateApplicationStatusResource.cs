using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.WorkFlow.Resources
{
    public class UpdateApplicationStatusResource
    {
        public string CurrentUsername { get; set; }
        public Guid ReferenceId { get; set; }
        public int UpdateStatus { get; set; }
        public string RoleName { get; set; }
        public string StatusName { get; set; }
        public int ApplicationId { get; set; }
        public string ApprovedAmount { get; set; }
        public string FundAdminApprovedAmount { get; set; }
        public string FundAdminComment { get; set; }
        public string SIAComment { get; set; }
    }
}
