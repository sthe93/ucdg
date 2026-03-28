using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Common;
using UDCG.Application.Common.Request.Models;

namespace UDCG.Application.Interface
{
    public interface IExternalRequestHelper
    {
        string MakeFormDataRequest(ApiInfoModel apiInfo, Common.Request.Models.RequestInfoFormDataModel requestInfoFormData);
        string MakeJsonDataRequest(ApiInfoModel apiInfo, Common.Request.Models.RequestInfoJsonModel requestInfoJsonData);
        //string MakeJsonDataRequestNoToken(ApiInfoModel apiInfo, RequestInfoJsonModel requestInfoJsonData);
    }
}
