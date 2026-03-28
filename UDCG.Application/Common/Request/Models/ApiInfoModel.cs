using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Common.Request.Models
{
    //public class ApiInfoModel
    //{
    //    public string BaseUrl { get; set; }
    //    public string AuthUrl { get; set; }
    //    public string Username { get; set; }
    //    public string Password { get; set; }
    //    public bool IsFormUrlEncoded { get; set; }
    //}

    public class ApiIntegrationCircleModel
    {
        public string BaseUrl { get; set; }
        public string AuthUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsFormUrlEncoded { get; set; }
    }
}
