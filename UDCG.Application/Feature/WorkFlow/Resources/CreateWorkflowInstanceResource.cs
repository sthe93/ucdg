using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.WorkFlow.Resources
{
    public class CreateWorkflowInstanceResource
    {
        public int WorkflowDefinitionId { get; set; }
        public string CreatedBy { get; set; }
    }
}
