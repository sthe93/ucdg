using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application.Resources;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface IDocumentRepository
    {
        Task<List<UploadDocumentViewModel>> Add(List<UploadDocumentViewModel> model);
        Task<List<UploadDocumentViewModel>> AddV2(List<UploadDocumentViewModel> model);
        Task<List<UploadDocumentViewModel>> GetDocumentsByApplicationId(int applicationsId);
        Task<List<Documents>> GetDocumentsListByApplicationsId(int applicationsId);
        byte[] GetDocumentById(int documentId);
        Task<byte[]> GetDocumentByIdV2(int documentId);
        Task<int> GetApplicationsDocumentsCountById(int applicationsId);
        Task<List<UploadDocumentViewModel>> DeleteDocumentById(int documentId);
        Task<List<MotivationLetterReadModel>> CreateMotivationalLetterDoc(List<MotivationLetterReadModel> model);
    }
}
