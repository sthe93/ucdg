using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UDCG.Application.Feature.Roles.Interfaces;

namespace UCDG.Infrastructure.Helpers
{
    public class RequestSet : IRequestSet
    {
        public string BaseUrl { get; set; }
        public string AuthUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsFormUrlEncoded { get; set; }

        public string ExecuteAsFormData(string controller, string httpVerb, HttpContent payLoad)
        {
            try
            {
                var token = ClientTokenHelper.GetToken(AuthUrl, Username, Password, IsFormUrlEncoded);

                using (var client = ClientTokenHelper.HttpClient(token))
                {
                    HttpResponseMessage response = null;
                    if (httpVerb.Equals(HttpVerb.Post))
                        response = client.PostAsync(BaseUrl + controller, payLoad).Result;
                    if (httpVerb.Equals(HttpVerb.Put))
                        response = client.PutAsync(BaseUrl + controller, payLoad).Result;
                    if (httpVerb.Equals(HttpVerb.Get))
                        response = client.GetAsync(BaseUrl + controller).Result;
                    if (httpVerb.Equals(HttpVerb.Delete))
                        response = client.DeleteAsync(BaseUrl + controller).Result;

                    return response?.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public string ExecuteAsJson(string controller, string httpVerb, object payLoad)
        {
            try
            {
                var token = ClientTokenHelper.GetToken(AuthUrl, Username, Password, IsFormUrlEncoded);

                using (var client = ClientTokenHelper.HttpClient(token))
                {
                    HttpResponseMessage response = null;
                    if (httpVerb.Equals(HttpVerb.Post))
                        response = client.PostAsJsonAsync(BaseUrl + controller, payLoad).Result;
                    if (httpVerb.Equals(HttpVerb.Put))
                        response = client.PutAsJsonAsync(BaseUrl + controller, payLoad).Result;
                    if (httpVerb.Equals(HttpVerb.Get))
                        response = client.GetAsync(BaseUrl + controller).Result;
                    if (httpVerb.Equals(HttpVerb.Delete))
                        response = client.DeleteAsync(BaseUrl + controller).Result;

                    return response?.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }


    }
}
