using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace UDCG.Application.Interface
{
    public interface IRequest
    {
        string BaseUrl { get; set; }
        string AuthUrl { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        bool IsFormUrlEncoded { get; set; }
        string ExecuteAsFormData(string controller, string httpVerb, HttpContent payLoad);
        string ExecuteAsJson(string controller, string httpVerb, object payLoad);
        string ExecuteAsJsonNoToken(string controller, string httpVerb, object payLoad);
    }
}
