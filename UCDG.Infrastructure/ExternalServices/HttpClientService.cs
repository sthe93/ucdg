using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UDCG.Application.Interface;

namespace UCDG.Infrastructure.ExternalServices
{
    public class HttpClientService : IHttpClientService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public async Task<HttpResponseMessage> HttpFilesPostAsync(string url, MultipartFormDataContent form, string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.PostAsync(url, form);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<T> HttpGetAsync<T>(string url, string token = "")
        {
            if (!String.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseBody);
        }
    }
}
