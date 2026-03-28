using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Persistence;
using UDCG.Application.Common.Constants;
using UDCG.Application.Feature.Application.Interface;
using UDCG.Application.Interface;
using System.Linq;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Emails.Interface;
using AutoMapper;
using UDCG.Application.Feature.Application;
using UCDG.Persistence.Repositories;
using Microsoft.AspNetCore.DataProtection.Repositories;
using UCDG.Persistence.Enums;
using UDCG.Application.Feature.Application.Resources;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;

namespace UCDG.Infrastructure.ExternalServices
{
    public class WorkFlowIntegrationService : IWorkFlowIntegrationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IApplicationStatusRepository _applicationStatusRepository;
        private readonly UCDGDbContext _context;
        private readonly IEmailSentRepository _emailRepository;
        private readonly IMapper _mapper;
        private readonly UserStoreDbContext _userStore;

        public WorkFlowIntegrationService(IApplicationRepository applicationRepository, IApplicationStatusRepository applicationStatusRepository, UCDGDbContext context, IEmailSentRepository emailRepository, IMapper mapper, UserStoreDbContext userStore)
        {
            _applicationRepository = applicationRepository;
            _applicationStatusRepository = applicationStatusRepository;
            _context = context;
            _emailRepository = emailRepository;
            _mapper = mapper;
            _userStore = userStore;

        }

        public async Task<ApplicationStatus> GetApplicationStatusByRoleName(string roleName, string status, string username)
        {
            var applicationStatus = new ApplicationStatus();

            if (string.Equals(roleName, UserRolesConstants.HOD, StringComparison.CurrentCultureIgnoreCase))
            {
                if (username != "")
                {
                    var userDetails = _applicationRepository.GetUserDetailsByUserName(username);

                    if (userDetails != null)
                    {
                        var isMec = _applicationRepository.CheckIfPersonIsMEC(userDetails.Result.StaffNumber);

                        if (await isMec)
                        {
                            if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                            {
                                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.PendingApprovalByFA);
                                if (res != null)
                                    applicationStatus = res;
                            }
                        }
                        else
                        {
                            if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                            {
                                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedByHOD);
                                if (res != null)
                                    applicationStatus = res;
                            }
                        }
                    }
                }

            }
            if (roleName.ToLower() == UserRolesConstants.FundAdministrator.ToLower())
            {
                if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                {
                    var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedByFundAdmin);
                    if (res != null)
                        applicationStatus = res;
                }
            }
            //if (roleName.ToLower() == UserRolesConstants.ViceDean.ToLower())
            //{
            //    if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
            //    {
            //        var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedbyViceDean);
            //        if (res != null)
            //            applicationStatus = res;
            //    }
            //}

            if (string.Equals(roleName, UserRolesConstants.ViceDean, StringComparison.CurrentCultureIgnoreCase))
            {
                if (username != "")
                {
                    var userDetails = _applicationRepository.GetUserDetailsByUserName(username);

                    if (userDetails != null)
                    {
                        var isMec = _applicationRepository.CheckIfPersonIsMEC(userDetails.Result.StaffNumber);
                        if (await isMec)
                        {
                            if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                            {
                                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.PendingApprovalByFA);
                                if (res != null)
                                    applicationStatus = res;
                            }
                        }
                        else
                        {
                            if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                            {
                                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedbyViceDean);
                                if (res != null)
                                    applicationStatus = res;
                            }
                        }
                    }
                }

            }
            if (roleName.ToLower() == UserRolesConstants.SIADirector.ToLower())
            {
                if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                {
                    var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedBySIADirector);
                    if (res != null)
                        applicationStatus = res;
                }
            }
            if (roleName.ToLower() == UserRolesConstants.FinancialBusinessPartner.ToLower())
            {
                if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                {
                    var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedByFinancialBusinessPartner);
                    if (res != null)
                        applicationStatus = res;
                }
            }
            //
            if (status.ToLower() == ApplicationStatuesConstants.Declined.ToLower())
            {
                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.Declined);
                if (res != null)
                    applicationStatus = res;
            }
            if (status.ToLower() == ApplicationStatuesConstants.ReturnedRorInfo.ToLower())
            {
                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ReturnedRorInfo);
                if (res != null)
                    applicationStatus = res;
            }
            //

            return applicationStatus;
        }

        public async Task<ApplicationStatus> GetApplicationStatusByRoleNameV2(string roleName, string status, UserStoreUser userDetails)
        {
            var applicationStatus = new ApplicationStatus();

            if (string.Equals(roleName, UserRolesConstants.HOD, StringComparison.CurrentCultureIgnoreCase))
            {
                if (userDetails != null)
                {
                    var isMec = _applicationRepository.CheckIfPersonIsMEC(userDetails.HRPostNumber);

                    if (await isMec)
                    {
                        if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                        {
                            var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.PendingApprovalByFA);
                            if (res != null)
                                applicationStatus = res;
                        }
                    }
                    else
                    {
                        if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                        {
                            var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.PendingApprovalByViceDean);
                            if (res != null)
                                applicationStatus = res;
                        }
                    }
                }

            }
            if (roleName.ToLower() == UserRolesConstants.FundAdministrator.ToLower())
            {
                if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                {
                    var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedByFundAdmin);
                    if (res != null)
                        applicationStatus = res;
                }
            }


            if (string.Equals(roleName, UserRolesConstants.ViceDean, StringComparison.CurrentCultureIgnoreCase))
            {
                    if (userDetails != null)
                    {
                        var isMec = _applicationRepository.CheckIfPersonIsMEC(userDetails.HRPostNumber);
                        if (await isMec)
                        {
                            if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                            {
                                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.PendingApprovalByFA);
                                if (res != null)
                                    applicationStatus = res;
                            }
                        }
                        else
                        {
                            if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                            {
                                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.PendingApprovalByFA);
                                if (res != null)
                                    applicationStatus = res;
                            }
                        }
                    }
            }
            if (roleName.ToLower() == UserRolesConstants.SIADirector.ToLower())
            {
                if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                {
                    var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedBySIADirector);
                    if (res != null)
                        applicationStatus = res;
                }
            }
            if (roleName.ToLower() == UserRolesConstants.FinancialBusinessPartner.ToLower())
            {
                if (status.ToLower() == ApplicationStatuesConstants.Approved.ToLower())
                {
                    var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ApprovedByFinancialBusinessPartner);
                    if (res != null)
                        applicationStatus = res;
                }
            }
            //
            if (status.ToLower() == ApplicationStatuesConstants.Declined.ToLower())
            {
                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.Declined);
                if (res != null)
                    applicationStatus = res;
            }
            if (status.ToLower() == ApplicationStatuesConstants.ReturnedRorInfo.ToLower())
            {
                var res = await _applicationStatusRepository.ApplicationStatusByName(ApplicationStatuesConstants.ReturnedRorInfo);
                if (res != null)
                    applicationStatus = res;
            }
            return applicationStatus;
        }

        public string UpdateStatus(ApplicationStatus status, Guid referenceId, string username, string roleName)
        {
            var application = _context.Applications.Include(o => o.ApplicationStatus).FirstOrDefault(o => o.ReferenceId == referenceId);
            User approverName = _context.Users.FirstOrDefault(o => o.Username.ToLower().Trim() == username.ToLower().Trim());
            try
            {
                var newStatus = new ApplicationStatus();
                newStatus = status;
                if (application == null) return "Not found";
                application.ApplicationStatus = newStatus;
                application.LastModifierUsername = username;
                application.LastModifiedDate = DateTime.Now;

                _context.SaveChanges();

                var applicationViewModel = _mapper.Map<ApplicationDetailsViewModel>(application);

                var IsApproverisMec = CheckIfNextApproverIsMEC(approverName.StaffNumber);

                if ((!IsApproverisMec))
                {
                    _ = SendEmail(status, applicationViewModel, roleName, approverName);
                }


                if (newStatus.StatusName.Equals(ApplicationStatuesConstants.ApprovedByHOD, StringComparison.CurrentCultureIgnoreCase))
                {
                    var IsNextApprover_isMec = CheckIfNextApproverIsMEC(approverName.HODStaffNUmber);
                    if (IsNextApprover_isMec == true)
                    {
                        var newStatusTemp = new ApplicationStatus();
                        newStatusTemp = new ApplicationStatus { Status = "Pending Approval by UCDG_Fund_Admin", StatusName = "Pending Approval by FA", ApplicationStatusId = 31 };
                        if (application == null) return "Not found";
                        application.ApplicationStatus = newStatusTemp;
                        application.LastModifierUsername = username;
                        application.LastModifiedDate = DateTime.Now;

                        _context.SaveChanges();

                        _ = SendEmail(newStatusTemp, applicationViewModel, roleName, approverName);
                    }
                }

                return "Successfully updated";
            }
            catch (Exception e)
            {
                _applicationRepository.SaveErrorLog("UpdateStatus", e.Message, "");

                return "Internal server error. " + e.Message;
            }
        }

        public bool UpdateStatusV2(ApplicationStatus status, Guid referenceId, UserStoreUser user, string role)
        {
            var application = _context.Applications.Include(o => o.ApplicationStatus).FirstOrDefault(o => o.ReferenceId == referenceId);
            User approverName = MapUserStoreUser();
             User MapUserStoreUser()
            {
                return new User
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FirstName = user.Name,
                    Surname = user.Surname,
                    Title = user.Title,
                    StaffNumber = user.HRPostNumber
                };
            }

            try
            {
                var newStatus = new ApplicationStatus();
                newStatus = status;
                if (application == null) return false;
                application.ApplicationStatus = newStatus;
                application.LastModifierUsername = approverName.Username;
                application.LastModifiedDate = DateTime.Now;

                _context.SaveChanges();

                var applicationViewModel = _mapper.Map<ApplicationDetailsViewModel>(application);

                var IsApproverisMec = CheckIfNextApproverIsMEC(approverName.StaffNumber);

                if ((!IsApproverisMec))
                {
                    _ = SendEmail(status, applicationViewModel, role, approverName);
                }


                if (newStatus.StatusName.Equals(ApplicationStatuesConstants.ApprovedByHOD, StringComparison.CurrentCultureIgnoreCase))
                {
                    var IsNextApprover_isMec = CheckIfNextApproverIsMEC(approverName.StaffNumber);
                    if (IsNextApprover_isMec == true)
                    {
                        var newStatusTemp = new ApplicationStatus();
                        newStatusTemp = new ApplicationStatus { Status = "Pending Approval by UCDG_Fund_Admin", StatusName = "Pending Approval by FA", ApplicationStatusId = 31 };
                        if (application == null) return false;
                        application.ApplicationStatus = newStatusTemp;
                        application.LastModifierUsername = approverName.Username;
                        application.LastModifiedDate = DateTime.Now;

                        _context.SaveChanges();

                        _ = SendEmail(newStatusTemp, applicationViewModel, role, approverName);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _applicationRepository.SaveErrorLog("UpdateStatus", e.Message, "");

                return false;
            }
        }

        public bool CheckIfNextApproverIsMEC(string empNo)
        {
            bool isMec = false;
            try
            {
                isMec = _context.MecMembers.Any(u => u.EmployeeNo == empNo);


                return isMec;

            }
            catch (Exception msg)
            {
                return false;
            }
        }


        public async Task SendEmail(ApplicationStatus status, ApplicationDetailsViewModel application, string roleName, User approverName)
        {

            try
            {
                var model = new ApplicationDetailsViewModel
                {
                    Id = application.Id,
                    ReferenceNumber = application.ReferenceNumber
                };

                var title = char.ToUpper(approverName.Title[0]) + "" + approverName.Title.Substring(1).ToLower();
                var fullName = title + " " + approverName.FirstName + " " + approverName.Surname;

                if (model.Id != 0)
                {
                    var result = LoadSendMailDbObjects(application, approverName, model);

                    // Access the results
                    Applications entity = result.Entity;
                    var temporaryApproverResult = result.TemporaryApproverResult;
                    var declineReason = result.DeclineReason;


                    var reason = string.IsNullOrEmpty(declineReason?.Comment)
                                    ? "Please login to view why your application was declined"
                                    : declineReason.Comment;

                    var applicantAddress = entity.Applicant.EmailAddress;
                    var approverUsername = approverName.Username;


                    if (entity != null)
                    {
                        if (temporaryApproverResult.Count > 0)
                        {

                            var lstTempApprover = new List<ApplicationApproversViewModel>();

                            var temporaryApprover = temporaryApproverResult
                                                    .Select(t => new
                                                    {
                                                        emailAddress = t.User.Email,
                                                        fullTitle = char.ToUpper(t.User.Title[0]) + t.User.Title.Substring(1).ToLower() + " " + t.User.Name + " " + t.User.Surname
                                                    }
                                                    ).FirstOrDefault();


                            lstTempApprover.Add(new ApplicationApproversViewModel { EmailAddress = temporaryApprover.emailAddress, FullTittle = temporaryApprover.fullTitle });

                            _emailRepository.SendNotificationEmail(model, approverName, entity, "Temporary Approver", "Application Approved - Temporary Approver HOD", "SendNewApplicationTemporaryApproverApprovedEmail", lstTempApprover, "");
                        }

                        SendNotificationEmailBasedOnRoleAndStatus(status, roleName, model, approverName, entity, approverUsername, reason);
                    }
                }
            }
            catch (Exception ex)
            {
                _applicationRepository.SaveErrorLog("SendEmail", "Error:" + ex.Message, "");

            }
        }

        public void SendNotificationEmailBasedOnRoleAndStatus(ApplicationStatus status, string roleName, ApplicationDetailsViewModel model, User approverName, Applications entity, string approverUsername, string reason)
        {
            var role = roleName.ToLower();
            var statusName = status.StatusName.ToLower();
            var lstResults = new List<ApplicationApproversViewModel>();

            void SendEmail(string recipientRole, string subject, string template, string approverRole, string reason = null)
            {
                _emailRepository.SendNotificationEmail(
                    model,
                    approverName,
                    entity,
                    recipientRole,
                    subject,
                    template,
                    GetApproversDetailsByStaffNo(entity, approverRole, approverUsername),
                    reason
                );
            }



            User approverDetails = _context.Users.FirstOrDefault(o => o.Username.ToLower().Trim() == approverUsername.ToLower().Trim());


            switch (role)
            {
                case var _ when role == UserRolesConstants.HOD.ToLower():
                    if (statusName == ApplicationStatuesConstants.ApprovedByHOD.ToLower())
                    {
                        SendEmail("HOD", "Application Approved - HOD", "NewApplicationHODApproved", AssignableUserRoles.HOD.GetDescription().ToLower().Trim());

                        var checkIfNextApproverIsMec = CheckIfNextApproverIsMEC(approverDetails.HODStaffNUmber);

                        if (!checkIfNextApproverIsMec)
                        {
                            SendEmail("Executive / Vice Dean", "New Application Received - Executive Dean / Vice Dean", "NewApplicationViceDeanReceived", AssignableUserRoles.ViceDean.GetDescription().ToLower().Trim());
                        }
                    }
                    else if (statusName == ApplicationStatuesConstants.Declined.ToLower())
                    {
                        SendEmail("HOD", "New Application Declined - HOD", "ApplicationHODDeclined", AssignableUserRoles.HOD.GetDescription().ToLower().Trim(), reason);
                    }
                    else if (statusName == ApplicationStatuesConstants.ReturnedRorInfo.ToLower())
                    {
                        SendEmail("HOD", "HOD Application RFI", "ApplicationHOD_RFI", AssignableUserRoles.HOD.GetDescription().ToLower().Trim(), reason);
                    }
                    else if (statusName == ApplicationStatuesConstants.PendingApprovalByFA.ToLower())
                    {
                        SendEmail("Fund Administrator", "New Application Received - Funding Admin", "ApplicationFundingAdminReceivedEmail", AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim());
                    }
                    break;

                case var _ when role == UserRolesConstants.ViceDean.ToLower():
                    if (statusName == ApplicationStatuesConstants.ApprovedbyViceDean.ToLower() || statusName == ApplicationStatuesConstants.PendingApprovalByFA.ToLower())
                    {
                        SendEmail("Applicant", "Application Approved - Executive Dean / Vice Dean", "ApplicationViceDeanApproved", AssignableUserRoles.Applicant.GetDescription().ToLower().Trim());
                        SendEmail("Fund Administrator", "New Application Received - Funding Admin", "ApplicationFundingAdminReceivedEmail", AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim());
                    }
                    else if (statusName == ApplicationStatuesConstants.Declined.ToLower())
                    {
                        SendEmail("Executive Dean / Vice Dean", "Application Declined", "ApplicationViceDeanDeclined", AssignableUserRoles.ViceDean.GetDescription().ToLower().Trim(), reason);
                    }
                    else if (statusName == ApplicationStatuesConstants.ReturnedRorInfo.ToLower())
                    {
                        SendEmail("Executive Dean / Vice Dean", "Executive / Vice Dean Application RFI", "ApplicationViceDeanRFI", AssignableUserRoles.ViceDean.GetDescription().ToLower().Trim());
                    }
                    break;

                case var _ when role == UserRolesConstants.FundAdministrator.ToLower():
                    if (statusName == ApplicationStatuesConstants.ApprovedByFundAdmin.ToLower())
                    {
                        SendEmail("Fund Administrator", "Application Approved - Fund Admin", "ApplicationApprovedFundAdmin", AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim());
                        SendEmail("SIA Director", "New Application Received - SIA Director", "ApplicationDirectorReceivedEmail", AssignableUserRoles.SIADirector.GetDescription().ToLower().Trim());
                    }
                    else if (statusName == ApplicationStatuesConstants.Declined.ToLower())
                    {
                        SendEmail("Fund Administrator", "New Application Declined", "ApplicationFundingAdminDeclinedEmail", AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim());
                    }
                    else if (statusName == ApplicationStatuesConstants.ReturnedRorInfo.ToLower())
                    {
                        SendEmail("Fund Administrator", "Fund Admin - Return for Information", "ApplicationFundAdminRFI", AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim(), reason);
                    }
                    break;

                case var _ when role == UserRolesConstants.SIADirector.ToLower():
                    if (statusName == ApplicationStatuesConstants.ApprovedBySIADirector.ToLower())
                    {
                        SendEmail("SIA Director", "Application Approved - SIA Director", "ApplicationDirectorApproveApplicant", AssignableUserRoles.SIADirector.GetDescription().ToLower().Trim());
                    }
                    else if (statusName == ApplicationStatuesConstants.Declined.ToLower())
                    {
                        SendEmail("SIA Director", "Application Declined - SIA Director", "ApplicationDirectorDeclinedEmail", AssignableUserRoles.SIADirector.GetDescription().ToLower().Trim(), reason);
                    }
                    else if (statusName == ApplicationStatuesConstants.ReturnedRorInfo.ToLower())
                    {
                        SendEmail("SIA Director", "Return for Information - SIA Director", "ApplicationRFI", AssignableUserRoles.SIADirector.GetDescription().ToLower().Trim());
                    }
                    break;

                case var _ when role == UserRolesConstants.FinancialBusinessPartner.ToLower():
                    if (statusName == ApplicationStatuesConstants.ApprovedByFinancialBusinessPartner.ToLower())
                    {
                        SendEmail("", "Application Approved - FBP", "NewApplicationFBPApproved", AssignableUserRoles.FinancialBusinessPartner.GetDescription().ToLower().Trim());
                    }
                    break;
            }
        }

        public List<ApplicationApproversViewModel> GetApproversDetailsByStaffNo(Applications entity, string sendToValue, string Currentusername)
        {
            var lstResults = new List<ApplicationApproversViewModel>();

            try
            {
                if (entity != null && sendToValue != "Applicant")
                {
                    var fullTitle = string.Empty;
                    var surname = string.Empty;

                    var roleName = string.Empty;
                    var emailAddress = "";

                    if (entity.Applicant.UserId != 0)
                    {
                        var applicantDetails = _userStore.Users.FirstOrDefault(c => c.UserId == entity.Applicant.UserId);

                        if (sendToValue.ToLower().Trim() == "hod")
                        {
                            if (!string.IsNullOrEmpty(applicantDetails.LineManagerStaffNumber))
                            {

                                var approverDetails = _userStore.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role).FirstOrDefault(u => u.HRPostNumber.ToLower().Trim() == applicantDetails.LineManagerStaffNumber.ToLower().Trim() && u.IsActive);

                                if (approverDetails != null)
                                {
                                    var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                                    fullTitle = title + " " + approverDetails.Name + " " + approverDetails.Surname;

                                    emailAddress = approverDetails.Email;
                                }
                            }
                        }
                        if (sendToValue.ToLower().Trim() == "executive / vice dean")
                        {
                            if (!string.IsNullOrEmpty(applicantDetails.ViceDeanStaffNumber))
                            {

                                var approverDetails = _userStore.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role).FirstOrDefault(u => u.HRPostNumber.ToLower().Trim() == applicantDetails.ViceDeanStaffNumber.ToLower().Trim() && u.IsActive);

                                if (approverDetails != null)
                                {
                                    var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                                    fullTitle = title + " " + approverDetails.Name + " " + approverDetails.Surname;
                                    emailAddress = approverDetails.Email;
                                }
                            }
                        }

                        if (sendToValue.ToLower().Trim() == "fund administrator")
                        {

                            var fundAdmin = _userStore.Roles.FirstOrDefault(r => r.Name.ToLower().Trim() == AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim());

                            if (fundAdmin != null)
                            {
                                if (fundAdmin.RoleId != 0)
                                {
                                    var userRole = _userStore.UserRoles.FirstOrDefault(u => u.RoleId == fundAdmin.RoleId && u.IsActive);
                                    var approverDetails = _userStore.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role).FirstOrDefault(u => u.UserId == userRole.UserId && u.IsActive);

                                    var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                                    fullTitle = title + " " + approverDetails.Name + " " + approverDetails.Surname;
                                    emailAddress = approverDetails.Email;
                                }
                            }
                        }

                        if (sendToValue.ToLower().Trim() == "sia director")
                        {
                            var fundAdmin = _userStore.Roles.FirstOrDefault(r => r.Name.ToLower().Trim() == AssignableUserRoles.SIADirector.GetDescription().ToLower().Trim());

                            if (fundAdmin != null)
                            {
                                if (fundAdmin.RoleId != 0)
                                {
                                    var userRole = _userStore.UserRoles.FirstOrDefault(u => u.RoleId == fundAdmin.RoleId && u.IsActive);
                                    var approverDetails = _userStore.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role).FirstOrDefault(u => u.UserId == userRole.UserId && u.IsActive);

                                    var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                                    fullTitle = title + " " + approverDetails.Name + " " + approverDetails.Surname;
                                    emailAddress = approverDetails.Email;
                                }
                            }
                        }

                        if (sendToValue.ToLower().Trim() == "temporary approver")
                        {
                            var fundAdmin = _userStore.Roles.FirstOrDefault(r => r.Name.ToLower().Trim() == AssignableUserRoles.TemporaryApprover.GetDescription().ToLower().Trim());

                            if (fundAdmin != null)
                            {
                                if (fundAdmin.RoleId != 0)
                                {
                                    var userRole = _userStore.UserRoles.FirstOrDefault(u => u.RoleId == fundAdmin.RoleId && u.IsActive);
                                    var approverDetails = _userStore.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role).FirstOrDefault(u => u.UserId == userRole.UserId && u.IsActive);

                                    var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                                    fullTitle = title + " " + approverDetails.Name + " " + approverDetails.Surname;
                                    emailAddress = approverDetails.Email;
                                }
                            }
                        }
                        if (sendToValue.ToLower().Trim() == "financial business partner")
                        {
                            var fundAdmin = _userStore.Roles.FirstOrDefault(r => r.Name.ToLower().Trim() == AssignableUserRoles.FinancialBusinessPartner.GetDescription().ToLower().Trim());

                            if (fundAdmin != null)
                            {
                                if (fundAdmin.RoleId != 0)
                                {
                                    var userRole = _userStore.UserRoles.FirstOrDefault(u => u.RoleId == fundAdmin.RoleId && u.IsActive);
                                    var approverDetails = _userStore.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role).FirstOrDefault(u => u.UserId == userRole.UserId && u.IsActive);

                                    var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                                    fullTitle = title + " " + approverDetails.Name + " " + approverDetails.Surname;
                                    emailAddress = approverDetails.Email;
                                }
                            }
                        }


                        fullTitle = fullTitle != string.Empty ? fullTitle : string.Empty;
                        emailAddress = emailAddress != string.Empty ? emailAddress : string.Empty;

                        if (fullTitle != string.Empty && emailAddress != string.Empty)
                        {
                            lstResults.Add(new ApplicationApproversViewModel { EmailAddress = emailAddress, FullTittle = fullTitle });
                        }
                    }
                }

                return lstResults;
            }
            catch (Exception ex)
            {

                return lstResults;
            }
        }

        public loadSendMailDbObjectsResult LoadSendMailDbObjects(ApplicationDetailsViewModel application, User approverName, ApplicationDetailsViewModel model)
        {
            try
            {
                var entity = _context.Applications.AsNoTracking()
                    .Include(c => c.Applicant)
                    .Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus)
                    .FirstOrDefault(f => f.Id == model.Id);

                var temporaryApproverResult = _context.TemporaryApproverApplications
                    .AsNoTracking()
                    //.Include(t => t.User)
                    .Where(t => t.Applications.Id == application.Id && t.UserId == approverName.UserId)
                    .Take(1)
                    .Select(t => new TemporaryApproverApplications
                    {
                        Id = t.Id,
                        ApprovedAs = t.ApprovedAs,
                        //User = t.User
                    })
                    .ToList();

                if (temporaryApproverResult.Count > 1) {
                    temporaryApproverResult[0].User = _userStore.Users.Where(u => u.UserId == approverName.UserId).FirstOrDefault();
                }

                var declineReason = _context.Comments.AsNoTracking()
                    .Where(c => c.ApplicationsId == application.Id)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault();

                return new loadSendMailDbObjectsResult
                {
                    Entity = entity,
                    TemporaryApproverResult = temporaryApproverResult,
                    DeclineReason = declineReason
                };
            }
            catch (Exception ex)
            {
                _applicationRepository.SaveErrorLog("SendEmail", "Error:" + ex.Message, "");

                return new loadSendMailDbObjectsResult
                {
                    Entity = null,
                    TemporaryApproverResult = null,
                    DeclineReason = null
                };
            }
        }


        public class loadSendMailDbObjectsResult
        {
            public Applications Entity { get; set; }
            public List<TemporaryApproverApplications> TemporaryApproverResult { get; set; }
            public Comments DeclineReason { get; set; }
        }

    }
}
