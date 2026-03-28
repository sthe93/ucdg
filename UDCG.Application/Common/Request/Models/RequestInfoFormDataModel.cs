using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace UDCG.Application.Common.Request.Models
{
    public class RequestInfoFormDataModel
    {
        public string Controller { get; set; }
        public string HttpVerb { get; set; }
        public HttpContent PayLoad { get; set; }
    }
}
