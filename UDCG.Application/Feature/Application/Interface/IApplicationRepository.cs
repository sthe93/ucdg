using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.WorkFlow.Resources;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface IApplicationRepository
    {
        Task<string> GetApprovedAs(int userId);
        Task<List<Applications>> GetInProgressApplications();
        Task<List<Applications>> GetTemporaryApproverApplications(int userId);
        Task<List<Applications>> GetMyApplications(int userId);
        Task<List<Applications>> GetHodApplications(int userId);
        Task<List<Applications>> GetHodProcessedApplications(int userId);
        Task<List<Applications>> GetViceDeanMyApplications(int userId);
        Task<List<Applications>> GetViceDeanProcessedApplications(int userId);
        Task<Applications> Add(ApplicationDetailsViewModel model);
        Task<Applications> Update(ApplicationDetailsViewModel model);
        Task<Applications> GetIncompleteApplicationByFundingIdUserId(int fundingCallId, int userId);
        Task<List<Applications>> GetCompleteApplicationsByUserName(int userId);
        Task<List<Applications>> GetIncompleteApplicationsByUserName(int userId);
        Task<List<Applications>> GetAllApplicationsByUserId(int userId);
        Task<Applications> GetCompleteApplicationByFundingIdUserId(int fundingCallId, int userId);
        Task<Applications> GetCompleteApplicationByFundingIdUsername(int fundingCallId, string username);
        Task<Applications> DownloadCompleteApplications(int applicationId);
        Task<List<Applications>> FilterGetMyApplications(SearchApplicationViewModel model);
        Task<List<Applications>> GetProcessedApplications();
        Task<List<Applications>> GetFbpProcessedApplications();
        Task<List<Applications>> GetFundAdminProcessedApplications();
        Task<List<Applications>> GetNewViceDeanApplications(int userId);
        Task<List<Applications>> GetNewFundingAdminApplications();
        Task<List<Applications>> GetFundingAdminApplications(int applicationStatusId);
        Task<List<Applications>> GetNewFbpApplications();
        Task<List<Applications>> GetNewSiaDirectorApplications();
        Task<List<Applications>> GetSiaDirectorProcessedApplications();
        Task<List<Applications>> GetNewApplications();
        Task<List<Applications>> GetDvcProcessedApplications(int userId);
        Task<List<Applications>> GetDvcNewApplications(int userId);
        Task<List<UCDG.Domain.Entities.Comments>> GetAllComment();
        Task<List<UCDG.Domain.Entities.Comments>> GetCommentsByApplicationId(int applicationId);
        Task<List<Applications>> GetFaCommentsByApplicationId(int applicationId);
        Task<UCDG.Domain.Entities.Comments> AddComments(AppplicationCommentVeiwModel model);

        Task<List<Applications>> GetApprovalLetter(int userId);
        Task<Applications> GetApplicationsById(int id);
        Task<Applications> UpdateApplicationStatus(Guid referenceId, int statusId);
        Task<Applications> UpdateAmountApproved(UpdateApplicationStatusResource statusUpdateModel);
        Task<Applications> UpdateSiaAmountApproved(UpdateApplicationStatusResource statusUpdateModel);
        Task<Applications> UpdateAwardDeclinedByApplicant(UpdateApplicationStatusResource statusUpdateModel);
        string UpdateAwardAcceptingByApplicant(UpdateApplicationStatusResource statusUpdateModel);
        void SendAwardLetterAcceptingByApplicant(UpdateApplicationStatusResource statusUpdateModel);
        void SendAwardLetterAcceptingByApplicantTemp();
        Task<String> UpdateWorkflowReferenceId(int applicationId);
        void SaveErrorLog(string functionality, string applicationError, string parameterValue);
        //void SendMailToAprovers(UpdateApplicationStatusResource statusUpdateModel, ApplicationDetailsViewModel model, List<User> user, Applications entity);
        void SendMailToApprover(UpdateApplicationStatusResource statusUpdateModel, ApplicationDetailsViewModel model, List<User> user, Applications entity);
        Task<bool> CheckIfPersonIsMEC(string empNo);

        Task<bool> IsApproverMec(User user, Applications entity);

        Task<bool> IsHodMec(User user, Applications entity);

        Task<bool> IsViceDeanMec(User user, Applications entity);


        Task<User> GetUserDetailsByUserName(string username);

        Task<List<Applications>> GetMyPreviousApplications(int userId);


        Task<List<MotivationLetterResponse>> CreateLinkUserMotivationLetter(List<MotivationLetterReadModel> model);
        Task<MotivationLetterResponse> GetLinkUserMotivationLetter(int usetId);

        Task<List<MotivationLetterReadModel>> CreateLinkUserMotivationLetterV2(List<MotivationLetterReadModel> model);



        //new
        Task<bool> SubmitApplication(ApplicationSubmissionModel model);
        Task<MotivationLetterReadModel> ViewMotivationLetterById(int documentId);
        Task<bool> DeleteMotivationLetterById(int documentId);
        Task<Applications> AddNewApplicationV2(ApplicationDetailsViewModel model,User user);
        Task<Applications> GetIncompleteApplicationByFundingIdUserAsync(int fundingCallId, int applicantUserStoreUserId, string username, string staffNumber);
        Task<Applications> SaveImprovementStaffQualifications(ApplicationDetailsViewModel model);
        Task<Applications> UpdateApplicantDetails(ApplicationDetailsViewModel model);

        Task<List<Applications>> GetAllIncompleteApplicationsByUserId(int userId);
        Task<List<Applications>> SearchCompleteApplicationsByUserId(int userId);
    }
}
