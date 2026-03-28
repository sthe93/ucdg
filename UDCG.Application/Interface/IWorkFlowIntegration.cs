using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UDCG.Application.Common;
using UDCG.Application.Feature.WorkFlow.Resources;

namespace UDCG.Application.Interface
{

    public interface IWorkFlowIntegration
    {
        public ApiWorkflowAPIModel Options { get; set; }
        public string Environment { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string WorkflowDefinitionName { get; set; }

        WorkFlowDefinition IsWorkFlowSetUp();
        Task<WorkflowInstanceResource> CreateWorkflowInstance(CreateWorkflowInstanceResource model);
        WorkflowEgineCurrentStatusResponse WorkflowEgineGetSurrentState(Guid referenceId);
        WorkflowEgineUpdateStatusResponse WorkflowEgineUpdateStatus(WorkflowEgineUpdateStatusResource model);
    }
}
