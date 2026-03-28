
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;

using System.Collections.Generic;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using UCDG.Domain.Entities;
using UCDG.Persistence.Enums;
using UDCG.Application.Common;
using UDCG.Application.Common.Constants;
using UDCG.Application.Feature.Application;

using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.Emails.Interface;
using UDCG.Application.Feature.Notifications.Resources;
using UDCG.Application.Feature.ProgressReport;
using UDCG.Application.Interface;

namespace UCDG.Persistence.Repositories
{
    public class EmailSentRepository : IEmailSentRepository
    {
        private readonly UCDGDbContext _context;
        private readonly UserStoreDbContext _userStore;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _config;
        private readonly ApiInfoModel _apiInfo;


        public EmailSentRepository(UCDGDbContext context, INotificationService notificationService, IConfiguration config, IOptions<ApiInfoModel> apiInfo, UserStoreDbContext userStore)
        {

            _context = context;
            _notificationService = notificationService;
            _config = config;
            _apiInfo = apiInfo.Value;
            _notificationService.ApiInfo = _apiInfo;
            _userStore = userStore;
        }

        public void SaveEmailErrorLog(string functionality, string applicationError, string parameterValue)
        {

            var errorDetails = new ErrorLogs
            {
                Functionality = functionality,
                ApplicationError = applicationError,
                DateCreated = DateTime.Now,

            };

            _context.ErrorLogs.Add(errorDetails);
            _context.SaveChanges();
        }


        public List<string> LstGenerateReminderDates()
        {
            var lstReminderDates = new List<string>();

            try
            {
                var reminderStartDate = _config.GetSection("AppSettings:ProgressReportReminderStartDate").Value;

                var reminderStartDateValue = DateTime.Parse(DateTime.Now.Year + reminderStartDate);

                var reminderEndDate = _config.GetSection("AppSettings:ProgressReportReminderEndDate").Value;

                var reminderEndDateValue = DateTime.Parse(DateTime.Now.Year + 1 + reminderEndDate);

                var firstMonday = reminderStartDateValue;
                while (firstMonday.DayOfWeek != DayOfWeek.Monday)
                {
                    firstMonday = firstMonday.AddDays(1);
                }

                for (var date = firstMonday; date <= reminderEndDateValue; date = date.AddDays(14))
                {

                    if (!IsLastWeekOfJanuary(date))
                    {
                        lstReminderDates.Add(date.Date.AddHours(6).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                    }
                }

                return lstReminderDates;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("lstGenerateReminderDates()", "Error: Unable to Generate ReminderDates " + ex.Message, "");
                return lstReminderDates;
            }
        }


        private static bool IsLastWeekOfJanuary(DateTime date)
        {
            if (date.Month != 1) return false;
            var lastDayOfJanuary = new DateTime(date.Year, 1, 31);
            return (lastDayOfJanuary - date).TotalDays < 7;
        }



        public async Task<int> GetMessageIdByReminderEmailSubmissionV2(ApplicationDetailsViewModel model, User user, Applications entity, string sendTo, string subject, string emailTemplate)
        {

            var messageRecipients = 0;

            try
            {



                var reminderDates = LstGenerateReminderDates();

                var (_, emailList) = MessageRecipientList(user.EmailAddress);


                if (reminderDates.Count <= 0) return messageRecipients;
                var msg = new NotificationResourceModel
                {
                    MessageBody = GetProgressReportRemindermailBody(entity),
                    Subject = subject,
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    isReminderOnly = true,
                    reminder = reminderDates.ToArray()
                };


                var response = _notificationService.SendAsyncWithResponse(msg);

                if (response == null) return messageRecipients;

                var parsedResponse = JObject.Parse(response);
                messageRecipients = (int)parsedResponse["messageRecipients"];


                var mrd = new MessageRecipientDetails
                {
                    ApplicationId = entity.Id,
                    MessageRecipientId = messageRecipients,
                    CreatedDate = DateTime.Now
                };

                await _context.MessageRecipientDetails.AddAsync(mrd);
                await _context.SaveChangesAsync();



                return messageRecipients;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("GetMessageIdByReminderEmailSubmission()", $"Error: Unable to Get Message Id By Reminder Email Submission {ex.Message}", "");
                return messageRecipients;
            }
        }

        public int GetMessageIdByReminderEmailSubmission(ApplicationDetailsViewModel model, Applications entity, string sendTo, string subject, string emailTemplate)
        {

            var messageRecipients = 0;

            try
            {



                var reminderDates = LstGenerateReminderDates();

                var (_, emailList) = MessageRecipientList(entity.Applicant.EmailAddress);


                if (reminderDates.Count <= 0) return messageRecipients;
                var msg = new NotificationResourceModel
                {
                    MessageBody = GetProgressReportRemindermailBody(entity),
                    Subject = subject,
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    isReminderOnly = true,
                    reminder = reminderDates.ToArray()
                };


                var response = _notificationService.SendAsyncWithResponse(msg);

                if (response == null) return messageRecipients;

                var parsedResponse = JObject.Parse(response);
                messageRecipients = (int)parsedResponse["messageRecipients"];
                return messageRecipients;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("GetMessageIdByReminderEmailSubmission()", $"Error: Unable to Get Message Id By Reminder Email Submission {ex.Message}", "");
                return messageRecipients;
            }
        }

        public void DeActivateProgressReportSubmissionReminder(int applicationId)
        {
            try
            {
                var entity = _context.MessageRecipientDetails
                              .Where(f => f.ApplicationId == applicationId)
                              .FirstOrDefaultAsync()
                              .GetAwaiter()
                              .GetResult();

                _notificationService.SendAsyncReminder(entity.MessageRecipientId);
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("ActivateProgressReportSubmissionReminderEmail()", "Error: Unable to Activate ProgressReport Submission Reminder Email " + ex.Message, "");

            }
        }
        public NotificationResourceModel DeActivateProgressReportSubmissionReminderV2(int applicationId)
        {
            try
            {
                var entity = _context.MessageRecipientDetails
                              .Where(f => f.ApplicationId == applicationId)
                              .FirstOrDefaultAsync()
                              .GetAwaiter()
                              .GetResult();

                _notificationService.SendAsyncReminder(entity.MessageRecipientId);

                return null;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("De ActivateProgressReportSubmissionReminderEmail()", "Error: Unable to Activate ProgressReport Submission Reminder Email " + ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicationHodEmailBody(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity != null)
                {
                    var sendTo = "HOD";
                    var hodEmailAddress = "";
                    if (entity.Applicant.UserId != 0)
                    {
                        var applicantDetails = await _userStore.Users.Where(c => c.UserId == entity.Applicant.UserId).FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(applicantDetails.LineManagerStaffNumber))
                        {

                            var hodDetails = await _userStore.Users.Where(u => u.HRPostNumber.ToLower().Trim() == applicantDetails.LineManagerStaffNumber.ToLower().Trim() && u.IsActive).FirstOrDefaultAsync();

                            if (hodDetails != null)
                            {
                                var title = char.ToUpper(hodDetails.Title[0]) + "" + hodDetails.Title.Substring(1).ToLower();

                                sendTo = title + " " + hodDetails.Name + " " + hodDetails.Surname;

                                hodEmailAddress = hodDetails.Email;
                            }
                        }

                    }

                    var emailList = new List<MessageRecipient>();

                    if (!string.IsNullOrEmpty(hodEmailAddress))
                    {
                        emailList = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = hodEmailAddress }
                            };
                    }


                    var body = GetNewApplicationHodEmailBody(entity, sendTo);

                    var msg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Received -HOD ",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = emailList,
                        reminder = reminderValue

                    };
                    _notificationService.SendAsync(msg);

                    return msg;
                }

                return null;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationHODEmailBody", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendApplicationRfiEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;
                var emailList = new List<MessageRecipient>();

                emailList = new List<MessageRecipient>
                {
                    new MessageRecipient() { EmailAddress = entity.Applicant.EmailAddress }
                };

                var body = GetApplicationRfiEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Request for Information",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendApplicationRFIEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendNewApplicantEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                var applicantAddress = entity.Applicant.EmailAddress;

                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;
                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantAddress }
                            };


                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetNewApplicantEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };

                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Submitted",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDvcEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;
                var emailList = new List<MessageRecipient>();

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                testEmails?.Split(";").ToList().ForEach(email =>
                {
                    emailList.Add(new MessageRecipient() { EmailAddress = email });
                });

                var body = GetNewApplicantDvcEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application DVC Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };

                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDVCEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDvcApprovedEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;
                var emailList = new List<MessageRecipient>();

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                testEmails?.Split(";").ToList().ForEach(email =>
                {
                    emailList.Add(new MessageRecipient() { EmailAddress = email });
                });

                var body = GetNewApplicantDvcApprovedEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application DVC Approved Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };

                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDVCApprovedEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDvcDeclinedEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity != null)
                {


                    var emailList = new List<MessageRecipient>();

                    var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                    testEmails?.Split(";").ToList().ForEach(email =>
                    {
                        emailList.Add(new MessageRecipient() { EmailAddress = email });
                    });

                    var body = GetNewApplicantDvcDeclinedEmailBody(entity);

                    var msg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application DVC Declined Submitted",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = emailList,
                        reminder = reminderValue
                    };

                    _notificationService.SendAsync(msg);
                    return msg;
                }

                return null;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDVCDeclinedEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDvcrfiEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;
                var emailList = new List<MessageRecipient>();

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                testEmails?.Split(";").ToList().ForEach(email =>
                {
                    emailList.Add(new MessageRecipient() { EmailAddress = email });
                });

                var body = GetNewApplicantDvcrfiEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Application DVC RFI Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };

                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDVCRFIEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDeanEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;
                var emailList = new List<MessageRecipient>();

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                testEmails?.Split(";").ToList().ForEach(email =>
                {
                    emailList.Add(new MessageRecipient() { EmailAddress = email });
                });

                var body = GetNewApplicantDeanEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Dean Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };

                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDeanEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDeanApprovedEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;
                var emailList = new List<MessageRecipient>();

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                testEmails?.Split(";").ToList().ForEach(email =>
                {
                    emailList.Add(new MessageRecipient() { EmailAddress = email });
                });

                var body = GetNewApplicantDeanApprovedEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Dean Approved Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDeanApprovedEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDeanDeclinedEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;
                var emailList = new List<MessageRecipient>();

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                testEmails?.Split(";").ToList().ForEach(email =>
                {
                    emailList.Add(new MessageRecipient() { EmailAddress = email });
                });

                var body = GetNewApplicantDeanDeclinedEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Dean Declined Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };

                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDeanDeclinedEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicantDeanRfiEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var emailList = new List<MessageRecipient>();
                if (entity == null) return null;
                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                if (testEmails != null)
                    testEmails.Split(";").ToList().ForEach(email =>
                    {
                        emailList.Add(new MessageRecipient() { EmailAddress = email });
                    });

                var body = GetNewApplicantDeanRfiEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Application Dean RFI Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };

                _notificationService.SendAsync(msg);
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicantDeanRFIEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendNewApplicationHodApprovedEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                var applicantAddress = entity.Applicant.EmailAddress;

                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new() { EmailAddress = ucdgEmailAddress }
                            };
                            }

                            break;
                        }
                }


                var body = GetNewApplicationHodApprovedEmailBody(entity, approverName);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Application Approved - HOD",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Application Approved - HOD",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationHODApprovedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendApplicationHodDeclinedEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                var reason = "";
                var applicantAddress = entity.Applicant.EmailAddress;
                reason = !string.IsNullOrEmpty(declineReason.Comment) ? declineReason.Comment : "Please login to view why your application was declined";


                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;
                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetApplicationHodDeclinedEmailBody(entity, approverName, reason);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Declined",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Declined",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;


            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendApplicationHODDeclinedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendNewApplicationViceDeanReceivedEmail(ApplicationDetailsViewModel model)
        {
            try
            {

                var reminderValue = Array.Empty<string>();
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                var sendTo = "Executive / Vice Dean";
                var vdEmailAddress = "";

                if (entity.Applicant.UserId != 0)
                {
                    var applicantDetails = await _userStore.Users.Where(c => c.UserId == entity.Applicant.UserId).FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(applicantDetails.ViceDeanStaffNumber))
                    {
                        var viceDeanDetails = await _userStore.Users.Where(u => u.HRPostNumber.ToLower().Trim() == applicantDetails.ViceDeanStaffNumber.ToLower().Trim() && u.IsActive).FirstOrDefaultAsync();
                        var title = char.ToUpper(viceDeanDetails.Title[0]) + "" + viceDeanDetails.Title.Substring(1).ToLower();
                        sendTo = title + " " + viceDeanDetails.Name + " " + viceDeanDetails.Surname;
                        vdEmailAddress = viceDeanDetails.Email;
                    }

                }
                //Comments declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                if (entity == null) return null;
                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(vdEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = vdEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetNewApplicationViceDeanReceivedEmailBody(entity, sendTo);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Received - Executive Dean / Vice Dean",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Received - Executive Dean / Vice Dean",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }


                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationViceDeanReceivedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendApplicationViceDeanApprovedEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var applicantEmailAddress = entity.Applicant.EmailAddress;

                if (entity == null) return null;


                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                            };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                                {

                                    new MessageRecipient() { EmailAddress = applicantEmailAddress }
                                };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                                {
                                    new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                                };
                            }

                            break;
                        }
                }

                var body = GetApplicationViceDeanApprovedEmailBody(entity, approverName);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Application Approved - Executive Dean / Vice Dean",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Application Approved - Executive Dean / Vice Dean",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendApplicationViceDeanApprovedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendApplicationFundAdminApprovedEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {

                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var applicantEmailAddress = entity.Applicant.EmailAddress;
                if (entity == null) return null;
                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetApplicationFundAdminApprovedEmailBody(entity, approverName);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Application Approved - Fund Admin",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Application Approved - Fund Admin",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendApplicationFundAdminApprovedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendAwardLetterReadyEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications
                    .Include(c => c.Applicant)
                    .Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus)
                    .FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;

                var applicantEmailAddress = entity.Applicant.EmailAddress;
                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;
                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });

                        ucdgInbox = new List<MessageRecipient>
                {
                    new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = applicantEmailAddress }
                        };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                        };
                            }
                            break;
                        }
                }

                var body = GetAwardLetterReadyEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = $"Award Letter Ready for Action - {entity.ReferenceNumber}",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = $"Award Letter Ready for Action - {entity.ReferenceNumber}",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendAwardLetterReadyEmail", ex.Message, model.ReferenceNumber);
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendResubmittedApplicationToHODEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications
                    .Include(c => c.Applicant)
                    .Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus)
                    .FirstOrDefaultAsync(f => f.Id == model.Id);

                if (entity == null) return null;

                var sendTo = "HOD";
                var hodEmailAddress = "";

                if (entity.Applicant.UserId != 0)
                {
                    var applicantDetails = await _userStore.Users.Where(c => c.UserId == entity.Applicant.UserId).FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(applicantDetails.LineManagerStaffNumber))
                    {
                        var hodDetails = await _userStore.Users
                            .Where(u => u.HRPostNumber.ToLower().Trim() == applicantDetails.LineManagerStaffNumber.ToLower().Trim() && u.IsActive)
                            .FirstOrDefaultAsync();

                        if (hodDetails != null)
                        {
                            var title = char.ToUpper(hodDetails.Title[0]) + "" + hodDetails.Title.Substring(1).ToLower();
                            sendTo = title + " " + hodDetails.Name + " " + hodDetails.Surname;
                            hodEmailAddress = hodDetails.Email;
                        }
                    }
                }

                var emailList = new List<MessageRecipient>();
                if (!string.IsNullOrEmpty(hodEmailAddress))
                {
                    emailList = new List<MessageRecipient>
            {
                new MessageRecipient() { EmailAddress = hodEmailAddress }
            };
                }

                var body = GetResubmittedApplicationHODEmailBody(entity, sendTo);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = $"Application Resubmitted - {entity.ReferenceNumber}",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                return msg;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendResubmittedApplicationToHODEmail", ex.Message, model.ReferenceNumber);
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendApplicationViceDeanDeclinedEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
                var applicantEmailAddress = entity.Applicant.EmailAddress;
                var reason = "";
                reason = !string.IsNullOrEmpty(declineReason.Comment) ? declineReason.Comment : "Please login to view why your application was declined";
                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetApplicationViceDeanDeclinedEmailBody(entity, approverName, reason);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Application Declined",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Application Approved - Fund Admin",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendApplicationViceDeanDeclinedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendApplicationViceDeanRfiEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                var reason = "";
                var applicantEmailAddress = entity.Applicant.EmailAddress;
                reason = !string.IsNullOrEmpty(declineReason.Comment) ? declineReason.Comment : "Please login to view why your application was returned for information";

                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;
                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });

                        ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                            };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                                {

                                    new MessageRecipient() { EmailAddress = applicantEmailAddress }
                                };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                                {
                                    new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                                };
                            }

                            break;
                        }
                }

                var body = GetSendApplicationViceDeanRfiEmailBody(entity, approverName, reason);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Executive / Vice Dean Application RFI",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Executive / Vice Dean Application RFI",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;
            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendApplicationViceDeanRFIEmail", ex.Message, "");
                return null;
            }
        }



        public async Task<NotificationResourceModel> SendNewApplicationFundingAdminReceivedEmail(ApplicationDetailsViewModel model)
        {
            try
            {

                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                var sendTo = "Fund Administrator";
                var fundAdminEmailAddress = "";
                if (entity.Applicant.UserId != 0)
                {

                    var fundAdmin = await _userStore.Roles.Where(r => r.Name.ToLower().Trim() == AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim()).FirstOrDefaultAsync();

                    if (fundAdmin != null)
                    {
                        if (fundAdmin.RoleId != 0)
                        {
                            var userRole = await _userStore.UserRoles.Where(u => u.RoleId == fundAdmin.RoleId && u.IsActive).FirstOrDefaultAsync();
                            var fundAdminDetails = await _userStore.Users.Where(c => c.UserId == userRole.UserId && c.IsActive).FirstOrDefaultAsync();
                            var title = char.ToUpper(fundAdminDetails.Title[0]) + "" + fundAdminDetails.Title.Substring(1).ToLower();

                            sendTo = title + " " + fundAdminDetails.Name + " " + fundAdminDetails.Surname;
                            fundAdminEmailAddress = fundAdminDetails.Email;
                        }
                    }
                }


                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(fundAdminEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = fundAdminEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetSendNewApplicationFundingAdminReceivedEmailBody(entity, sendTo);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Received - Funding Admin",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Received - Funding Admin",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationFundingAdminReceivedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendNewApplicationFundingAdminDeclinedEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                var reason = "";
                var applicantEmailAddress = entity.Applicant.EmailAddress;
                reason = !string.IsNullOrEmpty(declineReason.Comment) ? declineReason.Comment : "Please login to view why your application was declined";


                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                            };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                                {

                                    new MessageRecipient() { EmailAddress = applicantEmailAddress }
                                };
                            }


                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                                {
                                    new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                                };
                            }

                            break;
                        }
                }

                var body = GetSendNewApplicationFundingAdminDeclinedEmailBody(entity, approverName, reason);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Declined",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Declined",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationFundingAdminDeclinedEmail", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendNewApplicationDirectorReceivedEmail(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);


                var sendTo = "Director";
                var directorEmailAddress = "";
                if (entity.Applicant.UserId != 0)
                {

                    var fundAdmin = await _userStore.Roles.Where(r =>
                        r.Name.ToLower().Trim() ==
                        AssignableUserRoles.SIADirector.GetDescription().ToLower().Trim()).FirstOrDefaultAsync();

                    if (fundAdmin != null)
                    {
                        if (fundAdmin.RoleId != 0)
                        {
                            var userRole = await _userStore.UserRoles
                                .Where(u => u.RoleId == fundAdmin.RoleId && u.IsActive).FirstOrDefaultAsync();
                            var fundAdminDetails = await _userStore.Users
                                .Where(c => c.UserId == userRole.UserId && c.IsActive).FirstOrDefaultAsync();
                            var title = char.ToUpper(fundAdminDetails.Title[0]) + "" +
                                        fundAdminDetails.Title.Substring(1).ToLower();

                            sendTo = title + " " + fundAdminDetails.Name + " " + fundAdminDetails.Surname;
                            directorEmailAddress = fundAdminDetails.Email;
                        }
                    }
                }



                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;
                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient()
                                    { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                            };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(directorEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                                {

                                    new MessageRecipient() { EmailAddress = directorEmailAddress }
                                };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                                {
                                    new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                                };
                            }

                            break;
                        }
                }

                var body = GetSendNewApplicationDirectorReceivedEmailBody(entity, sendTo);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "New Application Received - SIA Director",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgMsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Received - SIA Director",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgMsg);
                }

                return msg;


            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationFundingAdminDeclinedEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendSubmissionReportDueEmail(ProgressReportDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                if (model.Id == 0) return null;
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.ApplicationId);
                var applicantEmailAddress = entity.Applicant.EmailAddress;

                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient()
                                { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetSubmissionReportDueEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Progress Report Submitted",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Progress Report Submitted",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };

                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendSubmissionReportDueEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendFundingAdminReviewEmailBody(ProgressReportDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                if (model.Id == 0) return null;
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.ApplicationId);

                var fundingAdmin = await _userStore.Users.Include(c => c.UserRoles).ThenInclude(c => c.Role).Where(o => o.UserRoles.FirstOrDefault().Role.Name == UserRolesConstants.FundAdministrator).ToListAsync();

                if (entity == null) return null;
                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (fundingAdmin.Count > 0)
                            {
                                fundingAdmin.ForEach(email =>
                                {
                                    emailList.Add(new MessageRecipient() { EmailAddress = email.Email });
                                });
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetFundingAdminReviewEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Progress Report Review",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Progress Report Review",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };

                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendFundingAdminReviewEmailBody", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendApplicantRfiEmail(ProgressReportDetailsViewModel model)
        {
            try
            {

                var reminderValue = Array.Empty<string>();

                if (model.Id == 0) return null;
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.ApplicationId);
                var applicantEmailAddress = entity.Applicant.EmailAddress;
                {
                    var environ = _config.GetSection("AppSettings:Environment").Value;
                    var emailList = new List<MessageRecipient>();
                    var ucdgInbox = new List<MessageRecipient>();
                    var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                    var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                    switch (environ)
                    {
                        case EnvironmentConstants.Test:
                            testEmails?.Split(";").ToList().ForEach(email =>
                            {
                                emailList.Add(new MessageRecipient() { EmailAddress = email });
                            });


                            ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                            };
                            break;
                        case EnvironmentConstants.Production:
                            {
                                if (!string.IsNullOrEmpty(applicantEmailAddress))
                                {
                                    emailList = new List<MessageRecipient>
                                {

                                    new MessageRecipient() { EmailAddress = applicantEmailAddress }
                                };
                                }

                                if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                                {
                                    ucdgInbox = new List<MessageRecipient>
                                {
                                    new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                                };
                                }

                                break;
                            }
                    }

                    var reportComment = await _context.ProgressReportComments.Include(c => c.User).Include(c => c.ProgressReports).Where(f => f.ProgressReports.Id == model.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();


                    var body = GetApplicantReportRfiEmailBody(entity, reportComment.Comment);

                    var msg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Progress Report Returned For Information",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = emailList,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(msg);


                    if (environ.Equals(EnvironmentConstants.Production))
                    {
                        var ucdgInboxmsg = new NotificationResourceModel()
                        {
                            MessageBody = body,
                            Subject = "Progress Report Returned For Information",
                            Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                            ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                            MessageRecipient = ucdgInbox,
                            reminder = reminderValue
                        };

                        _notificationService.SendAsync(ucdgInboxmsg);
                    }

                    return msg;
                }

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendFundingAdminReviewEmailBody", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendFundingReportFinalizedEmail(ProgressReportDetailsViewModel model)
        {
            try
            {

                var reminderValue = Array.Empty<string>();

                if (model.Id == 0) return null;
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.ApplicationId);
                var applicantEmailAddress = entity.Applicant.EmailAddress;


                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetFundingReportFinalizedEmailBody(entity);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Progress Report Finalized",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);

                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Progress Report Finalized",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };

                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendFundingReportFinalizedEmail", ex.Message, "");
                return null;
            }
        }


        public async Task<NotificationResourceModel> SendNewApplicationDirectorDeclinedEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);

                var declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                var reason = "";
                var applicantEmailAddress = entity.Applicant.EmailAddress;
                reason = !string.IsNullOrEmpty(declineReason.Comment) ? declineReason.Comment : "Please login to view why your application was declined";

                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(applicantEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = applicantEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetSendNewApplicationDirectorDeclinedEmailBody(entity, approverName, reason);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Application Declined - SIA Director",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Application Declined - SIA Director",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);

                }

                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationDirectorDeclinedEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendNewApplicationFbpEmailBody(ApplicationDetailsViewModel model)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var fbpEmailAddress = "";
                if (entity == null) return null;
                {
                    var sendTo = "Financial Business Partner";

                    if (entity.Applicant.UserId != 0)
                    {

                        var fundAdmin = await _userStore.Roles.Where(r => r.Name.ToLower().Trim() == AssignableUserRoles.FinancialBusinessPartner.GetDescription().ToLower().Trim()).FirstOrDefaultAsync();

                        if (fundAdmin != null)
                        {
                            if (fundAdmin.RoleId != 0)
                            {
                                var userRole = await _userStore.UserRoles.Where(u => u.RoleId == fundAdmin.RoleId && u.IsActive).FirstOrDefaultAsync();
                                var fundAdminDetails = await _userStore.Users.Where(c => c.UserId == userRole.UserId && c.IsActive).FirstOrDefaultAsync();
                                var title = char.ToUpper(fundAdminDetails.Title[0]) + "" + fundAdminDetails.Title.Substring(1).ToLower();

                                sendTo = title + " " + fundAdminDetails.Name + " " + fundAdminDetails.Surname;
                                fbpEmailAddress = fundAdminDetails.Email;
                            }
                        }
                    }


                    var environ = _config.GetSection("AppSettings:Environment").Value;
                    var emailList = new List<MessageRecipient>();
                    var ucdgInbox = new List<MessageRecipient>();
                    var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;


                    var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                    switch (environ)
                    {
                        case "Test":
                            testEmails?.Split(";").ToList().ForEach(email =>
                            {
                                emailList.Add(new MessageRecipient() { EmailAddress = email });
                            });


                            ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                            };
                            break;
                        case EnvironmentConstants.Production:
                            {
                                if (!string.IsNullOrEmpty(fbpEmailAddress))
                                {
                                    emailList = new List<MessageRecipient>
                                {

                                    new MessageRecipient() { EmailAddress = fbpEmailAddress }
                                };
                                }

                                if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                                {
                                    ucdgInbox = new List<MessageRecipient>
                                {
                                    new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                                };
                                }

                                break;
                            }
                    }

                    var body = GetNewApplicationFbpReceivedEmailBody(entity, sendTo);

                    var msg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "New Application Received - Financial Business Partner",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = emailList,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(msg);


                    if (environ.Equals(EnvironmentConstants.Production))
                    {
                        var ucdgInboxmsg = new NotificationResourceModel()
                        {
                            MessageBody = body,
                            Subject = "New Application Received - Financial Business Partner",
                            Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                            ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                            MessageRecipient = ucdgInbox,
                            reminder = reminderValue
                        };

                        _notificationService.SendAsync(ucdgInboxmsg);
                    }

                    return msg;
                }

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendNewApplicationFBPEmailBody", ex.Message, "");
                return null;
            }
        }
        public async Task<NotificationResourceModel> SendApplicationDirectorRfiEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();

                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                var reason = "";
                var directorEmailAddress = entity.Applicant.EmailAddress;
                reason = !string.IsNullOrEmpty(declineReason.Comment) ? declineReason.Comment : "Please login to view why your application was returned for information";


                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                switch (environ)
                {
                    case "Test":
                        testEmails?.Split(";").ToList().ForEach(email =>
                        {
                            emailList.Add(new MessageRecipient() { EmailAddress = email });
                        });


                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                        };
                        break;
                    case EnvironmentConstants.Production:
                        {
                            if (!string.IsNullOrEmpty(directorEmailAddress))
                            {
                                emailList = new List<MessageRecipient>
                            {

                                new MessageRecipient() { EmailAddress = directorEmailAddress }
                            };
                            }

                            if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                            {
                                ucdgInbox = new List<MessageRecipient>
                            {
                                new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                            };
                            }

                            break;
                        }
                }

                var body = GetApplicationDirectorRfiEmailBody(entity, approverName, reason);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Return for Information - SIA Director",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);



                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Fund Admin - Return for Information",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }
                return msg;

            }
            catch (Exception ex)
            {
                SaveEmailErrorLog("SendApplicationDirectorRFIEmail", ex.Message, "");
                return null;
            }
        }

        public async Task<NotificationResourceModel> SendApplicationFundAdminRfiEmail(ApplicationDetailsViewModel model, string approverName)
        {
            try
            {
                var reminderValue = Array.Empty<string>();
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var declineReason = await _context.Comments.Where(c => c.ApplicationsId == entity.Id).OrderByDescending(c => c.Id).FirstOrDefaultAsync();

                var reason = "Please login to view why your application was returned for information";
                var applicantEmailAddress = entity.Applicant.EmailAddress;

                if (declineReason != null)
                {
                    reason = !string.IsNullOrEmpty(declineReason.Comment) ? declineReason.Comment : "Please login to view why your application was returned for information";
                }

                if (entity == null) return null;
                var environ = _config.GetSection("AppSettings:Environment").Value;
                var emailList = new List<MessageRecipient>();
                var ucdgInbox = new List<MessageRecipient>();
                var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;

                var testEmails = _config.GetSection("AppSettings:testEmails").Value;

                if (environ == "Test")
                {
                    testEmails.Split(";").ToList().ForEach(email =>
                    {
                        emailList.Add(new MessageRecipient() { EmailAddress = email });
                    });


                    ucdgInbox = new List<MessageRecipient>
                    {
                        new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                    };
                }

                if (environ == EnvironmentConstants.Production)
                {
                    if (!string.IsNullOrEmpty(applicantEmailAddress))
                    {
                        emailList = new List<MessageRecipient>
                        {

                            new MessageRecipient() { EmailAddress = applicantEmailAddress }
                        };
                    }

                    if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                    {
                        ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                        };
                    }
                }

                var body = GetApplicationFundAdminRfiEmailBody(entity, approverName, reason);

                var msg = new NotificationResourceModel()
                {
                    MessageBody = body,
                    Subject = "Fund Admin - Return for Information",
                    Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                    ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                    MessageRecipient = emailList,
                    reminder = reminderValue
                };
                _notificationService.SendAsync(msg);


                if (environ.Equals(EnvironmentConstants.Production))
                {
                    var ucdgInboxmsg = new NotificationResourceModel()
                    {
                        MessageBody = body,
                        Subject = "Fund Admin - Return for Information",
                        Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                        ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                        MessageRecipient = ucdgInbox,
                        reminder = reminderValue
                    };
                    _notificationService.SendAsync(ucdgInboxmsg);
                }

                return msg;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }

        private string GetNewApplicantEmailBody(Applications entity)
        {
            var body = "";
            if (entity == null) return body;

            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "NewApplicationSubmitted.html");
            if (string.IsNullOrEmpty(file)) return body;
            using (StreamReader reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApplicationEndDate}", entity.ApplicationEndDate != null ? entity.ApplicationEndDate.Value.ToShortDateString() : DateTime.Now.ToString(CultureInfo.InvariantCulture));

            return body;
        }

        private string GetNewApplicantDvcEmailBody(Applications entity)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "NewApplicationDVCReceived.html");
            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ReferenceNumber}",
                entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApplicationEndDate}",
                entity.ApplicationEndDate != null
                    ? entity.ApplicationEndDate.Value.ToShortDateString()
                    : DateTime.Now.ToString(CultureInfo.InvariantCulture));

            return body;

        }

        private static string GetNewApplicantDvcApprovedEmailBody(Applications entity)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDVCApproved.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            return body;
        }

        private static string GetNewApplicantDvcrfiEmailBody(Applications entity)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDVCRFI.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");

            return body;
        }

        private static string GetNewApplicantDvcDeclinedEmailBody(Applications entity)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDVCDeclined.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");

            return body;
        }

        private string GetNewApplicantDeanEmailBody(Applications entity)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "NewApplicationDeanReceived.html");
            if (!string.IsNullOrEmpty(file))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                body = body.Replace("{ApplicationEndDate}", entity.ApplicationEndDate != null ? entity.ApplicationEndDate.Value.ToShortDateString() : DateTime.Now.ToString(CultureInfo.InvariantCulture));
            }

            return body;
        }

        private string GetNewApplicantDeanApprovedEmailBody(Applications entity)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDeanApproved.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }

            return body;
        }

        private string GetNewApplicantDeanRfiEmailBody(Applications entity)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDeanRFI.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                }
            }

            return body;
        }

        private string GetNewApplicantDeanDeclinedEmailBody(Applications entity)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDeanDeclined.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                }
            }

            return body;
        }

        private string GetProgressReportRemindermailBody(Applications entity)
        {


            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;



            var body = "";
            if (entity != null)
            {


                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "SendApplicantProgressReportReminder.html");

                if (!string.IsNullOrEmpty(file))
                {
                    var nextYear = DateTime.Now.Year + 1;

                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber);
                    body = body.Replace("{CurrentYear}", nextYear.ToString());
                    body = body.Replace("{NextYear}", nextYear.ToString());
                    body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);


                }
            }

            return body;
        }


        private string GetApplicationRfiEmailBody(Applications entity)
        {
            var body = "";
            if (entity != null)
            {

                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationRFI.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber);
                }
            }

            return body;
        }
        private string GetNewApplicationHodEmailBody(Applications entity, string sendTo)
        {
            var body = "";
            if (entity != null)
            {

                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "NewApplicationHODReceived.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{FullName}", sendTo);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                }
            }

            return body;
        }
        public string GetNewApplicationHodApprovedEmailBody(Applications entity, string approverName)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "NewApplicationHODApproved.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{username}", approverName);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }

            return body;
        }
        public string GetApplicationHodDeclinedEmailBody(Applications entity, string approverName, string declineReason)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationHODDeclined.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{username}", approverName);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{Reason}", declineReason);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                }
            }

            return body;
        }
        public string GetNewApplicationViceDeanReceivedEmailBody(Applications entity, string sendTo)
        {
            string body = "";
            if (entity != null)
            {
                string file = Path.Combine(Environment.CurrentDirectory, @"Template\", "NewApplicationViceDeanReceived.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{FullName}", sendTo);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);

                }
            }

            return body;
        }
        public string GetApplicationViceDeanApprovedEmailBody(Applications entity, string approverName)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationViceDeanApproved.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{username}", approverName);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }

            return body;
        }
        public string GetApplicationFundAdminApprovedEmailBody(Applications entity, string approverName)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationApprovedFundAdmin.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{username}", approverName);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }

            return body;
        }
        public string GetApplicationViceDeanDeclinedEmailBody(Applications entity, string approverName, string reason)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationViceDeanDeclined.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{username}", approverName);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{Reason}", reason);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }

            return body;
        }

        public string GetSendApplicationViceDeanRfiEmailBody(Applications entity, string approverName, string reason)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationViceDeanRFI.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{username}", approverName);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{Reason}", reason);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }
            }

            return body;
        }


        public string GetSendNewApplicationFundingAdminReceivedEmailBody(Applications entity, string sendTo)
        {
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationFundingAdminReceivedEmail.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{FullName}", sendTo);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                }
            }

            return body;
        }
        public string GetSendNewApplicationFundingAdminDeclinedEmailBody(Applications entity, string approverName, string reason)
        {

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;

            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationFundingAdminDeclinedEmail.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }

                    var applicationLink = _config.GetSection("AppSettings:ApplicationUrl").Value;

                    body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);

                    body = body.Replace("{username}", approverName);
                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
                    body = body.Replace("{Reason}", reason);
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                }
            }

            return body;
        }


        public string GetSubmissionReportDueEmailBody(Applications entity)
        {

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;


            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "SubmissionReportDue.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }

                    var applicationLink = _config.GetSection("AppSettings:ApplicationUrl").Value;

                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");

                    body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);
                }
            }

            return body;
        }

        public string GetFundingAdminReviewEmailBody(Applications entity)
        {

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;
            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "FundAdminProgressReportReview.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }

                    var applicationLink = _config.GetSection("AppSettings:ApplicationUrl").Value;

                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);
                }
            }

            return body;
        }

        public string GetApplicantReportRfiEmailBody(Applications entity, string comments)
        {

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;


            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicantReportRFI.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }

                    var applicationLink = _config.GetSection("AppSettings:ApplicationUrl").Value;

                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
                    body = body.Replace("{Reason}", comments);
                    body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);
                }
            }

            return body;
        }

        public string GetFundingReportFinalizedEmailBody(Applications entity)
        {

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;

            var body = "";
            if (entity != null)
            {
                var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "FundingReportSubmission.html");

                if (!string.IsNullOrEmpty(file))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        body = reader.ReadToEnd();
                    }

                    var applicationLink = _config.GetSection("AppSettings:ApplicationUrl").Value;

                    body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
                    body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");

                    body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);
                }
            }

            return body;
        }

        public string GetApplicantFbpDeclineBody(Applications entity, string sendTo)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicantFBPDecliningAwardLetter.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{FullName}", sendTo);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ApprovedAmount}", string.Format("{0:C}", entity.ApprovedAmount));
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            return body;
        }


        public string GetEmailBody(Applications entity, string sendTo, string declineReason, string template)
        {

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;

            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", $"{template}.html");

            var nextYear = DateTime.Now.Year + 1;
            var currentYear = DateTime.Now.Year.ToString();

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }



            body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);

            body = body.Replace("{username}", sendTo);
            body = body.Replace("{FullName}", sendTo);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);

            if (entity.ApprovedAmount != null)
            {
                body = body.Replace("{ApprovedAmount}", FormatDecimalWithSpaces(int.Parse(entity.ApprovedAmount)));
            }

            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            body = body.Replace("{NextYear}", nextYear.ToString());
            body = body.Replace("{CurrentYear}", currentYear);
            body = body.Replace("{Reason}", declineReason);

            return body;
        }

        private static string FormatDecimalWithSpaces(int amount)
        {
            var finalAmountDecimal = (decimal)amount;

            var formattedAmount = finalAmountDecimal.ToString("#,##0.00", CultureInfo.InvariantCulture);

            formattedAmount = formattedAmount.Replace(',', ' ');

            return formattedAmount;
        }

        //ApplicantBody 
        public string GetApplicantDeclineBody(Applications entity, string sendTo)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicatDecliningAwardLetter.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{FullName}", sendTo);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ApprovedAmount}", string.Format("{0:C}", entity.ApprovedAmount));
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            return body;
        }




        public string GetSendNewApplicationDirectorReceivedEmailBody(Applications entity, string sendTo)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDirectorReceivedEmail.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{FullName}", sendTo);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);

            return body;
        }
        public string GetSendNewApplicationDirectorDeclinedEmailBody(Applications entity, string approverName, string reason)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationDirectorDeclinedEmail.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (StreamReader reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{username}", approverName);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{Reason}", reason);
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            return body;
        }



        public string GetNewApplicationFbpReceivedEmailBody(Applications entity, string sendTo)
        {
            var body = "";
            if (entity == null) return body;
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "NewApplicationFBPReceived.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{FullName}", sendTo);

            return body;
        }

        private string GetApplicationDirectorRfiEmailBody(Applications entity, string approverName, string reason)
        {
            var body = "";
            if (entity == null) return body;
            //string contentRootPath = _hostingEnvironment.ContentRootPath;
            //string file = Path.Combine(contentRootPath, @"Template\ApplicationRFI.html");
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationRFI.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{username}", approverName);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{Reason}", reason);
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber);
            body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            return body;
        }
        private string GetApplicationFundAdminRfiEmailBody(Applications entity, string approverName, string reason)
        {
            var body = "";
            if (entity == null) return body;
            //string contentRootPath = _hostingEnvironment.ContentRootPath;
            //string file = Path.Combine(contentRootPath, @"Template\ApplicationRFI.html");
            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationFundAdminRFI.html");

            if (string.IsNullOrEmpty(file)) return body;
            using (var reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{username}", approverName);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName  + " " + entity.Applicant.Surname));
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{Reason}", reason);
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{ApprovalDate}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            return body;
        }
        private string GetAwardLetterReadyEmailBody(Applications entity)
        {
            var body = "";
            if (entity == null) return body;

            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "AwardLetterReady.html");
            if (string.IsNullOrEmpty(file)) return body;

            using (StreamReader reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }

            // FIX: SubmittedDate is DateTime, not nullable
            var expiryDate = entity.SubmittedDate != DateTime.MinValue
                ? entity.SubmittedDate.AddDays(14)
                : DateTime.Now.AddDays(14);

            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName + " " + entity.Applicant.Surname));
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{ApprovedAmount}", entity.ApprovedAmount != null ? entity.ApprovedAmount : "0.00");
            body = body.Replace("{ExpiryDate}", expiryDate.ToString("dd MMMM yyyy"));

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;
            body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);

            return body;
        }

        private string GetResubmittedApplicationHODEmailBody(Applications entity, string sendTo)
        {
            var body = "";
            if (entity == null) return body;

            var file = Path.Combine(Environment.CurrentDirectory, @"Template\", "ApplicationResubmittedHOD.html");
            if (string.IsNullOrEmpty(file)) return body;

            using (StreamReader reader = new StreamReader(file))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{FullName}", sendTo);
            body = body.Replace("{ApplicantFullName}", (string)(entity.Applicant.FirstName + " " + entity.Applicant.Surname));
            body = body.Replace("{ReferenceNumber}", entity.ReferenceNumber != null ? entity.ReferenceNumber : "");
            body = body.Replace("{FundingCallName}", entity.FundingCalls.FundingCallName);
            body = body.Replace("{Department}", entity.Applicant.Department ?? "");
            body = body.Replace("{ResubmissionDate}", DateTime.Now.ToString("dd MMMM yyyy"));

            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ApplicationURL = _config.GetSection("AppSettings:ApplicationURL").Value;
            var ApplicationURLQA = _config.GetSection("AppSettings:ApplicationURLQA").Value;
            body = environ == EnvironmentConstants.Test ? body.Replace("{ApplicationURL}", ApplicationURLQA) : body.Replace("{ApplicationURL}", ApplicationURL);

            return body;
        }
        private Tuple<List<MessageRecipient>, List<MessageRecipient>> MessageRecipientList(string emailAddress)
        {
            var testEmails = _config.GetSection("AppSettings:testEmails").Value;
            var environ = _config.GetSection("AppSettings:Environment").Value;
            var ucdgEmailAdddress = _config.GetSection("AppSettings:UCDGInbox").Value;
            var emailList = new List<MessageRecipient>();
            var ucdgInbox = new List<MessageRecipient>();

            switch (environ)
            {
                case "Test":
                    testEmails.Split(";").ToList().ForEach(email =>
                    {
                        emailList.Add(new MessageRecipient() { EmailAddress = email });
                    });


                    ucdgInbox = new List<MessageRecipient>
                    {
                        new MessageRecipient() { EmailAddress = _config.GetSection("AppSettings:ApplicationEmail").Value }
                    };
                    break;
                case EnvironmentConstants.Production:
                    {
                        if (!string.IsNullOrEmpty(emailAddress))
                        {
                            emailList = new List<MessageRecipient>
                        {

                            new MessageRecipient() { EmailAddress = emailAddress }
                        };
                        }

                        if (!string.IsNullOrEmpty(ucdgEmailAdddress))
                        {
                            ucdgInbox = new List<MessageRecipient>
                        {
                            new MessageRecipient() { EmailAddress = ucdgEmailAdddress }
                        };
                        }

                        break;
                    }
            }

            return Tuple.Create(ucdgInbox, emailList);
        }



        public void SendNotificationEmail(ApplicationDetailsViewModel model, User user, Applications entity, string sendTo, string subject, string emailTemplate, List<ApplicationApproversViewModel> approverDetails, string reason)
        {
            string toEmailAddress;

            var reminderValue = new string[] { };


            if (approverDetails.Count > 0)
            {
                var approver = approverDetails[0];
                toEmailAddress = approver.EmailAddress;

                var title = char.ToUpper(approver.FullTittle[0]) + "" + approver.FullTittle.Substring(1).ToLower();


                // sendTo = char.ToUpper(approver.FullTittle[0])+"";
                sendTo = approver.FullTittle; //title + " " + approver.Fistname[0] + " " + approver.Surname[0];

            }
            else
            {
                toEmailAddress = user.EmailAddress;
                var title = char.ToUpper(user.Title[0]) + "" + user.Title.Substring(1).ToLower();
                sendTo = title + " " + user.FirstName + " " + user.Surname;
            }

            if (model.Id == 0) return;
            if (entity == null) return;
            var body = GetEmailBody(entity, sendTo, reason, emailTemplate);

            var (ucdgInbox, emailList) = MessageRecipientList(toEmailAddress);

            var msg = new NotificationResourceModel()
            {
                MessageBody = body,
                Subject = subject,
                Sender = _config.GetSection("AppSettings:ApplicationEmail").Value,
                ClientApplication = _config.GetSection("AppSettings:ApplicationName").Value,
                MessageRecipient = emailList,
                isReminderOnly = false,
                reminder = reminderValue
            };

            _notificationService.SendAsync(msg);

            msg.MessageRecipient = ucdgInbox;
            _notificationService.SendAsync(msg);


        }

    }
}


