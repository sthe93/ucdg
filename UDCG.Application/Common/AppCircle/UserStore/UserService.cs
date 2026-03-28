
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using UDCG.Application.Common.Request.Models;
using UDCG.Application.Feature.Resrouces;
using UDCG.Application.Interface;

namespace UDCG.Application.Common.AppCircle.UserStore
{
    public class UserService : IUserService
    {
        private readonly IExternalRequestHelper _request;

        public ApiInfoModel ApiInfo { get; set; }
        public UserService(IExternalRequestHelper request, IOptions<ApiInfoModel> apiInfo)
        {
            _request = request;
            ApiInfo = apiInfo.Value;
        }
        public UserStoreDto GetUserByUsername(string username)
        {
            var json = JsonConvert.SerializeObject(username);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestInfo = new RequestInfoJsonModel
            {
                Controller = $"Users/Verify/{username}/UCDG",
                HttpVerb = HttpVerb.Get,
                PayLoad = content
            };

            var response = _request.MakeJsonDataRequest(ApiInfo, requestInfo);
            return JsonConvert.DeserializeObject<UserStoreDto>(response);
        }
    }
}
