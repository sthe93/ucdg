using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.ProgressReport.Interface
{
    public interface IProgressReportRepository
    {
        Task<ProgressReports> Add(ProgressReportDetailsViewModel model);
        Task<ProgressReports> Update(ProgressReportDetailsViewModel model);
        Task<ProgressReportComments> AddComments(ProgressReportCommentsViewModel model);
        Task<ProgressReportComments> AddCommentsV2(ProgressReportCommentsViewModel model);
        Task<List<ProgressReportComments>> GetCommentsByReportId(int reportId);
        Task<List<ProgressReportComments>> GetCommentsListByApplicationsId(int applicationsId);
        Task<ProgressReports> FinalizeProgressReport(ProgressReportDetailsViewModel model);
        Task<ProgressReports> RFIProgressReport(ProgressReportDetailsViewModel model);
        Task<bool> GetReportStatus(int applicationsId);
        Task<bool> GetViewReportStatus(int applicationsId);
        Task<ProgressReportDetailsViewModel> DownloadCompleteProgressReport(int reportId);
        Task<ProgressReportDetailsViewModel> GetProgressReportByApplicationId(int applicationId);
        Task<List<ProgressReportDetailsViewModel>> GetProgressReportSubmitted(); 
        Task<List<Applications>> GetProgressReportPending(); 
        Task<List<CreateDocumentViewModel>> DocumentUpload(List<CreateDocumentViewModel> model);
        Task<ProgressReportDetailsViewModel> GetCompletedProgressReportByApplicationId(int applicationId);
        Task<List<CreateDocumentViewModel>> GetPrgressReportDocumentsByApplicationId(int applicationsId);
        byte[] GetProgressReportDocument(int documentId);
        Task<byte[]> GetProgressReportDocumentV2(int documentId);
        Task<List<CreateDocumentViewModel>> DeleteProgressReportDocument(int documentId);
        Task<List<CreateDocumentViewModel>> DocumentUploadV2(List<CreateDocumentViewModel> model);
    }
}
