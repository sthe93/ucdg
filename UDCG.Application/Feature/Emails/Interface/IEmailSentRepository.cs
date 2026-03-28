using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application;
using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.Notifications.Resources;
using UDCG.Application.Feature.ProgressReport;

namespace UDCG.Application.Feature.Emails.Interface
{
    public interface IEmailSentRepository
    {

        Task<int> GetMessageIdByReminderEmailSubmissionV2(ApplicationDetailsViewModel model, User user,
            Applications entity, string sendTo, string subject, string emailTemplate);
        int GetMessageIdByReminderEmailSubmission(ApplicationDetailsViewModel model, Applications entity, string sendTo, string subject, string emailTemplate);
       


        void DeActivateProgressReportSubmissionReminder(int applicationId);
        NotificationResourceModel DeActivateProgressReportSubmissionReminderV2(int applicationId);


        void SaveEmailErrorLog(string functionality, string applicationError, string parameterValue);
  
        Task<NotificationResourceModel> SendNewApplicantDvcApprovedEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantDvcDeclinedEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantDvcrfiEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantDvcEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantDeanApprovedEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantDeanDeclinedEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantDeanRfiEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantDeanEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicantEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendApplicationRfiEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicationHodEmailBody(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicationHodApprovedEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendApplicationHodDeclinedEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendNewApplicationViceDeanReceivedEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendApplicationViceDeanApprovedEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendApplicationViceDeanDeclinedEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendApplicationViceDeanRfiEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendNewApplicationFundingAdminReceivedEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendNewApplicationFundingAdminDeclinedEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendNewApplicationDirectorReceivedEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendSubmissionReportDueEmail(ProgressReportDetailsViewModel model);
        Task<NotificationResourceModel> SendFundingReportFinalizedEmail(ProgressReportDetailsViewModel model);
        Task<NotificationResourceModel> SendApplicantRfiEmail(ProgressReportDetailsViewModel model);
        Task<NotificationResourceModel> SendFundingAdminReviewEmailBody(ProgressReportDetailsViewModel model);
        
        Task<NotificationResourceModel> SendNewApplicationDirectorDeclinedEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendNewApplicationFbpEmailBody(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendApplicationDirectorRfiEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendApplicationFundAdminRfiEmail(ApplicationDetailsViewModel model, string approverName);
        Task<NotificationResourceModel> SendApplicationFundAdminApprovedEmail(ApplicationDetailsViewModel model, string approverName);


        void SendNotificationEmail(ApplicationDetailsViewModel model, User user, Applications entity, string sendTo, string subject, string emailTemplate, List<ApplicationApproversViewModel> approverDetails, string reason);
        Task<NotificationResourceModel> SendAwardLetterReadyEmail(ApplicationDetailsViewModel model);
        Task<NotificationResourceModel> SendResubmittedApplicationToHODEmail(ApplicationDetailsViewModel model);
    }
}
