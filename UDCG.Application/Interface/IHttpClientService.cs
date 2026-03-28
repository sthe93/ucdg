using System.Net.Http;
using System.Threading.Tasks;

namespace UDCG.Application.Interface
{
    public interface IHttpClientService
    {
        Task<T> HttpGetAsync<T>(string url, string token = "");
        Task<HttpResponseMessage> HttpFilesPostAsync(string url, MultipartFormDataContent form, string token = "");
    }
}
