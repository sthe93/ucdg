using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Common
{

    public class ApiInfoModel
    {
        public string BaseUrl { get; set; }
        public string AuthUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsFormUrlEncoded { get; set; }
    }


    public class ApiIntegrationCircleModel
    {
        public string BaseUrl { get; set; }
        public string AuthUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsFormUrlEncoded { get; set; }
    }
    public class ApiWorkflowAPIModel
    {
        public string BaseUrl { get; set; }
        public string AuthUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsFormUrlEncoded { get; set; }
        public string WorkflowDefinitionName { get; set; }
    }

    public class ApiIntegrationCircleAPICostCentreModel
    {
        public string BaseUrl { get; set; }
        public string AuthUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsFormUrlEncoded { get; set; }
    }
}
