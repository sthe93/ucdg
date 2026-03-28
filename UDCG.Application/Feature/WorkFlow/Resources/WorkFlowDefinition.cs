using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.WorkFlow.Resources
{
    public class WorkFlowDefinition
    {
        public int WorkflowDefinitionId { get; set; }
        public string WorkflowDefinitionName { get; set; }
        public string WorkflowReferenceId { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
