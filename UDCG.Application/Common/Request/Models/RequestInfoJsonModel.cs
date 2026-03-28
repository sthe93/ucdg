using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Common.Request.Models
{
    public class RequestInfoJsonModel
    {
        public string Controller { get; set; }
        public string HttpVerb { get; set; }
        public object PayLoad { get; set; }
    }
}
