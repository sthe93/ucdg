
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UDCG.Application.Common.AppCircle.DocumentStore;

namespace UDCG.Application.Interface
{
    public interface IAppCircleService
    {
        Task<HttpResponseMessage> UploadDocuments(List<DocumentCreationModel> documents);
        Task<AppCircleDocument> GetDocument(string GuidId);
    }
}
