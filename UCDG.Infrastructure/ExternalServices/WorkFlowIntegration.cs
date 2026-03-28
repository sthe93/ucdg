using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Infrastructure.Helpers;
using UDCG.Application.Common;
using UDCG.Application.Feature.Roles.Interfaces;
using UDCG.Application.Feature.WorkFlow.Resources;
using UDCG.Application.Interface;

namespace UCDG.Infrastructure.ExternalServices
{
    public class WorkFlowIntegration : IWorkFlowIntegration
    {
        private readonly IRequestSet _request;

        public ApiWorkflowAPIModel Options { get; set; }
        public string Environment { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string WorkflowDefinitionName { get; set; }

        public WorkFlowIntegration(IRequestSet request)
        {
            _request = request;
        }

        public WorkFlowDefinition IsWorkFlowSetUp()
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;
        

            var response = _request.ExecuteAsJson("workflow-definition/get-by-name/" + WorkflowDefinitionName, HttpVerb.Get, null);

            //var url = "workflow-definition/get-by-name/" + Uri.EscapeDataString(WorkflowDefinitionName);
            //var response = _request.ExecuteAsJson(url, HttpVerb.Get, null);


            if (!string.IsNullOrEmpty(response))
                return JsonConvert.DeserializeObject<WorkFlowDefinition>(response);

            return null;
        }
        public Task<WorkflowInstanceResource> CreateWorkflowInstance(CreateWorkflowInstanceResource model)
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;

            var response =  _request.ExecuteAsJson("workflow-egine/create", HttpVerb.Post, model);

            return Task.FromResult(JsonConvert.DeserializeObject<WorkflowInstanceResource>(response));
        }
        public WorkflowEgineUpdateStatusResponse WorkflowEgineUpdateStatus(WorkflowEgineUpdateStatusResource model)
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;

            var response = _request.ExecuteAsJson("workflow-egine/update-status", HttpVerb.Post, model);

            try
            {
                return JsonConvert.DeserializeObject<WorkflowEgineUpdateStatusResponse>(response);
            }
            catch (Exception e)
            {

                throw;
            }
        }
        public WorkflowEgineCurrentStatusResponse WorkflowEgineGetSurrentState(Guid referenceId) 
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;

            var response = _request.ExecuteAsJson("workflow-egine/currentstate/by-referenceid?ReferenceId=" + referenceId, HttpVerb.Get, null);

            return JsonConvert.DeserializeObject<WorkflowEgineCurrentStatusResponse>(response);
        }

    }
}
