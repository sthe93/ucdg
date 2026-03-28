
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UCDG.Infrastructure.Helpers;
using UDCG.Application.Common.AppCircle.DocumentStore;
using UDCG.Application.Interface;

namespace UCDG.Application.Interface
{
    public class AppCircleService(IHttpClientService httpClientService, IConfiguration configuration) : IAppCircleService
    {
        private readonly IHttpClientService httpClientService = httpClientService;
        private readonly IConfiguration configuration = configuration;
        private readonly string authUrl = configuration["AppCircleAPI:AuthUrl"];
        private readonly string username = configuration["APPCIRCLE_TOKEN_USER"];
        private readonly string password = configuration["APPCIRCLE_TOKEN_PASSWORD"];
        private readonly string baseUrl = configuration["AppCircleAPI:BaseUrl"];

        public async Task<AppCircleDocument> GetDocument(string GuidId)
        {
            var token = GetToken();
            var url = $"{this.baseUrl}Documents/{GuidId}";
            var result = await this.httpClientService.HttpGetAsync<AppCircleDocument>(url, token);
            return result;
        }

        public async Task<HttpResponseMessage> UploadDocuments(List<DocumentCreationModel> documents)
        {
            var token = GetToken();
            var url = $"{this.baseUrl}Documents/UploadDocuments";

            using var form = new MultipartFormDataContent();

            for (int i = 0; i < documents.Count; i++)
            {
                var doc = documents[i];

                if (doc.DocumentContent != null && doc.DocumentContent.Length > 0)
                {
                    var fileContent = new ByteArrayContent(doc.DocumentContent);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    form.Add(fileContent, $"documents[{i}].File", doc.OriginalDocumentName ?? $"doc_{i}.bin");
                }

                form.Add(new StringContent(doc.DocumentGuid.ToString()), $"documents[{i}].DocumentGuid");
                form.Add(new StringContent(doc.BatchGuid.ToString()), $"documents[{i}].BatchGuid");
                form.Add(new StringContent(doc.OriginalDocumentName ?? ""), $"documents[{i}].OriginalDocumentName");
                form.Add(new StringContent(doc.CreatedBy ?? "UCDP"), $"documents[{i}].CreatedBy");
                form.Add(new StringContent(doc.ModifiedBy ?? "UCDP"), $"documents[{i}].ModifiedBy");
                form.Add(new StringContent(doc.IsArchived.ToString()), $"documents[{i}].IsArchived");
            }

            // Await async call correctly
            var result = await this.httpClientService.HttpFilesPostAsync(url, form, token);

            return result;
        }

        #region Token
        private string GetToken()
        {
            var token = ClientTokenHelper.GetToken(authUrl, username, password, false);
            return token;
        }
        #endregion
    }
}
