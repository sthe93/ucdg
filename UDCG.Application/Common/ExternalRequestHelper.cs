using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Common.Request.Models;
using UDCG.Application.Interface;

namespace UDCG.Application.Common
{
    public class ExternalRequestHelper : IExternalRequestHelper
    {
        private readonly IRequest _request;

        public ExternalRequestHelper(IRequest request)
        {
            _request = request;
        }
        public string MakeFormDataRequest(ApiInfoModel apiInfo, RequestInfoFormDataModel requestInfoFormData)
        {
            _request.BaseUrl = apiInfo.BaseUrl;
            _request.AuthUrl = apiInfo.AuthUrl;
            _request.Username = apiInfo.Username;
            _request.Password = apiInfo.Password;
            _request.IsFormUrlEncoded = apiInfo.IsFormUrlEncoded;

            return _request.ExecuteAsFormData(requestInfoFormData.Controller, requestInfoFormData.HttpVerb, requestInfoFormData.PayLoad);
        }

        public string MakeJsonDataRequest(ApiInfoModel apiInfo, RequestInfoJsonModel requestInfoJsonData)
        {
            _request.BaseUrl = apiInfo.BaseUrl;
            _request.AuthUrl = apiInfo.AuthUrl;
            _request.Username = apiInfo.Username;
            _request.Password = apiInfo.Password;
            _request.IsFormUrlEncoded = apiInfo.IsFormUrlEncoded;

            return _request.ExecuteAsJson(requestInfoJsonData.Controller, requestInfoJsonData.HttpVerb, requestInfoJsonData.PayLoad);
        }
    }
}
