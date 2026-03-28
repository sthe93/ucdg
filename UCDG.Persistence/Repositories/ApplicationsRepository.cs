using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UCDG.Infrastructure.Interfaces;
using UCDG.Persistence.Enums;
using UDCG.Application.Common;
using UDCG.Application.Common.Constants;
using UDCG.Application.Feature.Application;
using UDCG.Application.Feature.Application.Interface;
using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.Emails.Interface;
using UDCG.Application.Feature.WorkFlow.Resources;
using UDCG.Application.Interface;
using Projects = UCDG.Domain.Entities.Projects;

namespace UCDG.Persistence.Repositories
{
    public class ApplicationsRepository : IApplicationRepository
    {
        private readonly IConfiguration _config;
        private readonly UCDGDbContext _context;
        private readonly UserStoreDbContext _userStoreDbContext;
        private readonly IEmailSentRepository _emailSentRepository;

        private readonly IWorkFlowIntegration _workFlowIntegration;
        private readonly ApiWorkflowAPIModel _options;
        private readonly IUserIdResolver _userIdResolver;

        public ApplicationsRepository(IConfiguration config, UCDGDbContext context, IEmailSentRepository emailSentRepository, IOptions<ApiWorkflowAPIModel> options, IWorkFlowIntegration workFlowIntegration, IUserIdResolver userIdResolver, UserStoreDbContext userStoreDbContext)
        {
            _config = config;
            _context = context;
            _emailSentRepository = emailSentRepository;

            _workFlowIntegration = workFlowIntegration;

            _options = options.Value;
            _userIdResolver = userIdResolver;
            _userStoreDbContext = userStoreDbContext;
        }

        private void UpdateWorkFlowRefId()
        {
            var workflowDefinitionIdValue = 4;

            var environ = _config.GetSection("AppSettings:Environment").Value;

            _workFlowIntegration.Options = _options;
            _workFlowIntegration.WorkflowDefinitionName = _options.WorkflowDefinitionName;



            var getApps = _context.Applications.Include(o => o.Applicant).Where(o => o.ReferenceId == Guid.Parse("00000000-0000-0000-0000-000000000000") && o.Username != null).Select(o => new { o.ReferenceId, o.Id, o.Username }).ToList();
            if (environ != EnvironmentConstants.Production)
            {
                workflowDefinitionIdValue = 2;
            }
            getApps.ForEach(o =>
            {

                var workFlowInst = new CreateWorkflowInstanceResource
                {
                    CreatedBy = o.Username,
                    WorkflowDefinitionId = workflowDefinitionIdValue
                };
                var workflowInstanceResource = _workFlowIntegration.CreateWorkflowInstance(workFlowInst).Result;
                var results = _context.Applications.Find(o.Id);
                if (results != null) results.ReferenceId = workflowInstanceResource.Id;

                _context.SaveChanges();
            });
        }




        public async Task<User> GetUserDetailsByUserName(string username)
        {
            try
            {
                var userDetails = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                return userDetails;

            }
            catch (Exception msg)
            {
                return null;
            }
        }



        public async Task<bool> CheckIfPersonIsMEC(string empNo)
        {
            bool isMec = false;
            try
            {
                isMec = await _context.MecMembers.AnyAsync(u => u.EmployeeNo == empNo);


                return isMec;

            }
            catch (Exception msg)
            {
                return false;
            }
        }


        public async Task<bool> IsApproverMec(User user, Applications entity)
        {

            try
            {
                var executiveManagementDesc = EmployeePersonTypeEnum.ExecutiveManagement.GetDescription().Trim().ToLower();
                var hodPersonType = entity.Applicant.HodPersonType.Trim().ToLower();
                var viceDeanPersonType = entity.Applicant.ViceDeanPersonType?.Trim().ToLower();

                if (entity.ApplicationStatus.Status == "Incomplete")
                {
                    if (hodPersonType == executiveManagementDesc)
                    {
                        return true;
                    }
                }
                else
                {
                    if (viceDeanPersonType == executiveManagementDesc)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception msg)
            {
                return false;
            }
        }




        public async Task<bool> IsViceDeanMec(User user, Applications entity)
        {
            bool isViceMec = false;

            try
            {
                var executiveManagementDesc = EmployeePersonTypeEnum.ExecutiveManagement.GetDescription().Trim().ToLower();
                var hodPersonType = entity.Applicant.HodPersonType?.Trim().ToLower();
                var viceDeanPersonType = entity.Applicant.ViceDeanPersonType?.Trim().ToLower();

                var viceDeanEmpNo = entity.Applicant.ViceDeanStaffNUmber?.Trim().ToLower();

                isViceMec = await _context.MecMembers.AnyAsync(u => u.EmployeeNo == viceDeanEmpNo);

                if (isViceMec == true)
                {
                    return true;
                }

                return false;
            }
            catch (Exception msg)
            {
                return false;
            }
        }

        public async Task<bool> IsHodMec(User user, Applications entity)
        {
            bool isHodMec = false;
            try
            {
                var executiveManagementDesc = EmployeePersonTypeEnum.ExecutiveManagement.GetDescription().Trim().ToLower();
                var hodPersonType = entity.Applicant.HodPersonType?.Trim().ToLower();
                var viceDeanPersonType = entity.Applicant.ViceDeanPersonType?.Trim().ToLower();

                var hodEmpNo = entity.Applicant.HODStaffNUmber?.Trim().ToLower();

                isHodMec = await _context.MecMembers.AnyAsync(u => u.EmployeeNo == hodEmpNo);

                if (isHodMec == true)
                {
                    return true;
                }

                return false;

            }
            catch (Exception msg)
            {
                return false;
            }
        }

        public async Task<List<Applications>> GetHodApplications(int userId)
        {
            try
            {
                var applications = new List<Applications>();


                var hodDetails = await _userStoreDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();


                if (await (CheckIfPersonIsMEC(hodDetails.HRPostNumber)))
                {
                    return applications;
                }


                if (string.IsNullOrEmpty(hodDetails.HRPostNumber))
                    return applications.OrderBy(c => c.ApplicationEndDate).ToList();
                {
                    var hodUsers = await _userStoreDbContext.Users.Where(u => u.LineManagerStaffNumber.ToLower().Trim() == hodDetails.HRPostNumber.ToLower().Trim()).ToListAsync();

                    foreach (var user in hodUsers)
                    {
                        var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.Applicant.UserId == user.UserId && o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.PendingApprovalByHod.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).OrderBy(c => c.ApplicationEndDate).ToListAsync();

                        applications.AddRange(res);
                    }
                }


                foreach (var application in applications)
                {
                    var fundingCall = application.FundingCalls;


                    if (fundingCall != null)
                    {
                        var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                        .ToListAsync();


                        fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    }
                }


                return applications.OrderBy(c => c.ApplicationEndDate).ToList();

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetHodProcessedApplications(int userId)
        {
            try
            {
                var applications = new List<Applications>();

                var hodDetails = await _userStoreDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();

                if (await (CheckIfPersonIsMEC(hodDetails.HRPostNumber)))
                {
                    return applications;
                }

                if (string.IsNullOrEmpty(hodDetails.HRPostNumber))
                    return applications.OrderBy(c => c.ApplicationEndDate).ToList();
                {
                    var hodUsers = await _userStoreDbContext.Users.Where(u => u.LineManagerStaffNumber.ToLower().Trim() == hodDetails.HRPostNumber.ToLower().Trim()).ToListAsync();

                    //  foreach (var user in hodUsers)
                    //  {




                    var res = await _context.Applications
                        .Include(o => o.ApplicationStatus)
                        .Include(o => o.Applicant)
                        .Include(o => o.FundingCalls)
                        .Where(o =>
                        //o.User.UserId == user.UserId  && 
                        (
                                         //o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.PendingApprovalByFa.GetDescription().ToLower().Trim()
                                         o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbyHOD.GetDescription().ToLower().Trim()
                                         //|| o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbySIADirector.GetDescription().ToLower().Trim()
                                         //  || o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbyFundAdmin.GetDescription().ToLower().Trim()
                                         // || o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbyViceDean.GetDescription().ToLower().Trim()

                                         || o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.PendingApprovalByFa.GetDescription().ToLower().Trim()
                         ))
                        .Where(o => o.ReferenceNumber != null)
                        //  .Where(o => o.ApplicationStartDate >= DateTime.Now.AddMonths(-24)) // Filter for DateSubmitted within the last 24 months

                        .OrderByDescending(o => o.ReferenceNumber)
                        .ToListAsync();

                    applications.AddRange(res);






                    //  }
                }

                foreach (var application in applications)
                {
                    var fundingCall = application.FundingCalls;


                    if (fundingCall != null)
                    {
                        var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                        .ToListAsync();


                        fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    }
                }



                return applications.OrderByDescending(o => o.ReferenceNumber).ToList();

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetFbpProcessedApplications()
        {
            try
            {
                var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbyFinancialBusinessPartner.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).ToListAsync();
                return res;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetFundAdminProcessedApplications()
        {
            try
            {
                var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.ApplicationStatus.Status.ToLower().Trim() != ApplicationStatusEnum.ApprovedbyFundAdmin.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).OrderByDescending(c => c.ReferenceNumber).ToListAsync();


                foreach (var application in res)
                {
                    var fundingCall = application.FundingCalls;


                    if (fundingCall != null)
                    {
                        var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                        .ToListAsync();


                        fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    }
                }

                return res;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetNewViceDeanApplications(int userId)
        {
            try
            {
                var applications = new List<Applications>();

                var viceDeanDetails = await _userStoreDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();


                if (await (CheckIfPersonIsMEC(viceDeanDetails.HRPostNumber)))
                {
                    return applications;
                }


                if (string.IsNullOrEmpty(viceDeanDetails.HRPostNumber)) return applications;
                {
                    var viceDeanUsers = await _userStoreDbContext.Users.Where(u => u.LineManagerStaffNumber.ToLower().Trim() == viceDeanDetails.HRPostNumber.ToLower().Trim()).ToListAsync();

                    foreach (var user in viceDeanUsers)
                    {
                        var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.Applicant.UserId == user.UserId && o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbyHOD.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).OrderByDescending(c => c.ReferenceNumber).ToListAsync();

                        applications.AddRange(res);
                    }
                }

                foreach (var application in applications)
                {
                    var fundingCall = application.FundingCalls;


                    if (fundingCall != null)
                    {
                        var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                        .ToListAsync();


                        fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    }
                }


                // return applications;

                return applications.OrderBy(c => c.ApplicationEndDate).ToList();

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetSiaDirectorProcessedApplications()
        {
            try
            {
                var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbySIADirector.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).OrderByDescending(o => o.ReferenceNumber).ToListAsync();

                foreach (var application in res)
                {
                    var fundingCall = application.FundingCalls;


                    if (fundingCall != null)
                    {
                        var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                        .ToListAsync();
                        fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    }
                }

                return res;




            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetViceDeanMyApplications(int userId)
        {
            try
            {
                var applications = new List<Applications>();

                var viceDeanDetails = await _userStoreDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
                if (await (CheckIfPersonIsMEC(viceDeanDetails.HRPostNumber)))
                {
                    return applications;
                }

                if (string.IsNullOrEmpty(viceDeanDetails.HRPostNumber))
                    return applications.OrderBy(c => c.ApplicationEndDate).ToList();
                {
                    var viceDeanUsers = await _userStoreDbContext.Users.Where(u => u.ViceDeanStaffNumber.ToLower().Trim() == viceDeanDetails.HRPostNumber.ToLower().Trim()).ToListAsync();

                    foreach (var user in viceDeanUsers)
                    {
                        var res = await _context.Applications
                             .Include(o => o.ApplicationStatus)
                             .Include(o => o.Applicant)
                             .Include(o => o.FundingCalls)
                             .Where(o => o.Applicant.UserId == user.UserId && o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.PendingApproval.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).OrderByDescending(c => c.ApplicationEndDate).ToListAsync();

                        applications.AddRange(res);
                    }
                }

                foreach (var application in applications)
                {
                    var fundingCall = application.FundingCalls;


                    if (fundingCall != null)
                    {
                        var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                        .ToListAsync();


                        fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    }
                }
                return applications.OrderByDescending(c => c.ReferenceNumber).ToList();

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<List<Applications>> GetViceDeanProcessedApplications(int userId)
        {
            try
            {
                var applications = new List<Applications>();

                var viceDeanDetails = await _userStoreDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();

                if (await (CheckIfPersonIsMEC(viceDeanDetails.HRPostNumber)))
                {
                    return applications;
                }

                if (string.IsNullOrEmpty(viceDeanDetails.HRPostNumber))
                    return applications.OrderBy(c => c.ApplicationEndDate).ToList();
                {
                    var viceDeanUsers = await _userStoreDbContext.Users.Where(u => u.ViceDeanStaffNumber.ToLower().Trim() == viceDeanDetails.HRPostNumber.ToLower().Trim()).ToListAsync();

                    foreach (var user in viceDeanUsers)
                    {
                        var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o =>
                            //o.User.UserId == user.UserId && 
                            o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.PendingApprovalByFa.GetDescription().ToLower().Trim()
                            || o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbyViceDean.GetDescription().ToLower().Trim())
                            .Where(o => o.ReferenceNumber != null).OrderByDescending(c => c.ReferenceNumber).ToListAsync();

                        applications.AddRange(res);
                    }
                }


                foreach (var application in applications)
                {
                    var fundingCall = application.FundingCalls;


                    if (fundingCall != null)
                    {
                        var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                        .ToListAsync();


                        fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    }
                }



                return applications
                    .DistinctBy(a => a.Id)
                    .OrderByDescending(c => c.ReferenceNumber).ToList();

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetMyApplications(int userId)
        {



            var ids = await _userIdResolver.ResolveUserIdsAsync(userId);
            //var apps = await _context.Applications
            //            .Where(a => ids.Contains(a.User.UserId))
            //            .ToListAsync();


            var ids2 = await _userIdResolver.ResolveUserIdsAsync(userId); // e.g., [299, 3208]
            if (ids2 == null || ids2.Count == 0)
                return new List<Applications>();

            var idSet = new HashSet<int>(ids2);

            var query = _context.Applications
                .AsNoTracking()
                .Where(a => idSet.Contains(a.UserId.Value))
                .Include(a => a.ApplicationStatus)
                .Include(a => a.Applicant)
                .Include(a => a.FundingCalls)
                .OrderByDescending(a => a.ApplicationEndDate);


            var sql = query.ToQueryString();
            var res = await query.ToListAsync();

            //var res = await _context.Applications
            //    .Include(o => o.ApplicationStatus)
            //    .Include(o => o.User)
            //    .Include(o => o.FundingCalls)
            //    .Where(o => o.User.UserId == userId)
            //    .OrderByDescending(c => c.ApplicationEndDate).ToListAsync();


            // foreach (var application in query)
            //  {
            //                     var fundingCall = application.FundingCalls;


            //                     if (fundingCall != null)
            //                     {
            //                         var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
            //                                         .ToListAsync();


            //                         fundingCall.FundingCallProjects =await GetApplicationProjectsByApplicationId(application.Id);

            //                     }
            //                 }

            //                         foreach (var application in res)
            //                 {
            //                     var fundingCall = application.FundingCalls;


            //                     if (fundingCall != null)
            //                     {
            //                         var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
            //                                         .ToListAsync();


            //                         fundingCall.FundingCallProjects =await GetApplicationProjectsByApplicationId(application.Id);

            //                     }
            //}



            foreach (var application in res)
            {
                var fundingCall = application.FundingCalls;
                if (fundingCall != null)
                {
                    // Example of awaited work:
                    var appProjects = await _context.ApplicationsProjects
                        .Where(fc => fc.ApplicationsId == application.Id)
                        .ToListAsync();

                    fundingCall.FundingCallProjects =
                        await GetApplicationProjectsByApplicationId(application.Id);
                }
            }

            return res.OrderByDescending(c => c.ApplicationEndDate).ToList();
        }

        public async Task<string> GetApprovedAs(int userId)
        {
            var res = await _context.TemporaryApproverApplications
                .Where(o => o.User.UserId == userId).Select(c => c.ApprovedAs).FirstOrDefaultAsync();
            return res;
        }

        public async Task<List<Applications>> GetInProgressApplications()
        {
            var res = await _context.Applications
                .Include(o => o.ApplicationStatus)
                .Include(o => o.Applicant)
                .Include(o => o.FundingCalls)
                .Where(o => o.ReferenceNumber != null).OrderByDescending(c => c.ReferenceNumber).ToListAsync();
            return res;
        }

        public async Task<List<Applications>> GetTemporaryApproverApplications(int userId)
        {
            var applicationsList = new List<Applications>();

            var temporaryApproverApplications = await _context.TemporaryApproverApplications.Where(s => s.User.UserId == userId).Include(c => c.User).Include(a => a.Applications).ToListAsync();

            foreach (var item in temporaryApproverApplications)
            {
                var application = await _context.Applications.Include(c => c.ApplicationStatus).Include(c => c.Applicant).Include(c => c.FundingCalls).Where(a => a.Id == item.Applications.Id).Where(c => c.ReferenceNumber != null).FirstOrDefaultAsync();
                applicationsList.Add(application);
            }

            return applicationsList;

        }

        public async Task<Applications> Add(ApplicationDetailsViewModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
                var status = await _context.ApplicationStatus.FirstOrDefaultAsync(s => s.Status.ToLower() == ApplicationStatusEnum.Incomplete.GetDescription().ToLower());
                var fundingCall = await _context.FundingCalls.FirstOrDefaultAsync(f => f.Id == model.FundingCallDetailsId);

                if (user == null || fundingCall == null) return null;
                var applicant = new Applications
                {
                    Applicant = user,
                    FundingCalls = fundingCall,
                    ApplicationStatus = status,
                    LastSavedStep = model.LastSavedStep,
                    ApplicationStartDate = model.StartDate,
                    ApplicantCategory = model.ApplicantCategory,
                    AppointmentCategory = model.AppointmentCategory,
                    FundingStartDate = model.FundingStartDate,
                    FundingEndDate = model.FundingEndDate,
                    CostCentreName = model.CostCentreName,
                    CostCentreNumber = model.CostCentreNumber,
                    PreviousFundingYear = model.PreviousFundingYear,
                    PreviousFundingAmount = model.PreviousFundingAmount,
                    PreviousFundingOutcome = model.PreviousFundingOutcome
                };

                await _context.Applications.AddAsync(applicant);

                await _context.SaveChangesAsync();

                return applicant;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }



        public async Task<Applications> AddNewApplicationV2(ApplicationDetailsViewModel model, User user)
        {
            try
            {
                // var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);



                var status = await _context.ApplicationStatus.FirstOrDefaultAsync(s => s.Status.ToLower() == ApplicationStatusEnum.Incomplete.GetDescription().ToLower());
                var fundingCall = await _context.FundingCalls.FirstOrDefaultAsync(f => f.Id == model.FundingCallDetailsId);

                if (user == null || fundingCall == null) return null;
                var applicant = new Applications
                {
                    //Applicant = user,
                    UserId = null,    // 3386,
                    FundingCalls = fundingCall,
                    ApplicationStatus = status,
                    LastSavedStep = model.LastSavedStep,
                    ApplicationStartDate = model.StartDate,
                    ApplicantCategory = model.ApplicantCategory,
                    AppointmentCategory = model.AppointmentCategory,
                    FundingStartDate = model.FundingStartDate,
                    FundingEndDate = model.FundingEndDate,
                    CostCentreName = model.CostCentreName,
                    CostCentreNumber = model.CostCentreNumber,
                    PreviousFundingYear = model.PreviousFundingYear,
                    PreviousFundingAmount = model.PreviousFundingAmount,
                    PreviousFundingOutcome = model.PreviousFundingOutcome,
                    ApplicantUserStoreUserId = user.UserId,
                    Username = user.Username
                };

                await _context.Applications.AddAsync(applicant);

                await _context.SaveChangesAsync();

                return applicant;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<Applications> GetIncompleteApplicationByFundingIdUserId(int fundingCallId, int userId)
        {
            try
            {
                if (fundingCallId == 0 || userId == 0) return null;
                var application = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Applicant.UserId == userId && f.FundingCalls.Id == fundingCallId && f.ApplicationStatus.Status.ToLower() == ApplicationStatusEnum.Incomplete.GetDescription().ToLower());

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        //new

        public async Task<Applications> GetIncompleteApplicationByFundingIdUserAsync(int fundingCallId, int applicantUserStoreUserId, string username, string staffNumber)
        {
            try
            {
                if (fundingCallId <= 0 || applicantUserStoreUserId <= 0)
                    return null;

                username = username?.Trim().ToLower();
                staffNumber = staffNumber?.Trim();

                var incompleteStatus = ApplicationStatusEnum.Incomplete.GetDescription();

                int? legacyUserId = null;

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(staffNumber))
                {
                    legacyUserId = await _context.Users
                        .Where(u => u.Username == username && u.StaffNumber == staffNumber)
                        .Select(u => (int?)u.UserId)
                        .FirstOrDefaultAsync();
                }

                var application = await _context.Applications
                    .Include(c => c.Applicant)
                    .Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus)
                    .FirstOrDefaultAsync(f =>
                        f.FundingCallsId == fundingCallId &&
                        f.ApplicationStatus.Status == incompleteStatus &&
                        (
                            f.ApplicantUserStoreUserId == applicantUserStoreUserId
                            ||
                            (legacyUserId.HasValue && f.UserId == legacyUserId.Value)
                        ));

                return application;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving incomplete application.", ex);
            }
        }


        public async Task<Applications> GetCompleteApplicationByFundingIdUserId(int fundingCallId, int userId)
        {
            try
            {
                if (fundingCallId == 0 || userId == 0) return null;
                var application = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Applicant.UserId == userId && f.FundingCalls.Id == fundingCallId && f.ApplicationStatus.Status.ToLower() != ApplicationStatusEnum.Incomplete.GetDescription().ToLower());

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<Applications> GetCompleteApplicationByFundingIdUsername(int fundingCallId, string username)
        {
            try
            {
                if (fundingCallId == 0 || string.IsNullOrEmpty(username)) return null;
                var application = await _context.Applications.Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Username == username && f.FundingCalls.Id == fundingCallId && f.ApplicationStatus.Status.ToLower() != ApplicationStatusEnum.Incomplete.GetDescription().ToLower());

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<Applications> DownloadCompleteApplications(int applicationId)
        {
            try
            {
                if (applicationId == 0) return null;
                var application = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == applicationId && f.ApplicationStatus.Status.ToLower() != ApplicationStatusEnum.Incomplete.GetDescription().ToLower());

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<List<Applications>> GetCompleteApplicationsByUserName(int userId)
        {
            try
            {
                if (userId == 0) return null;
                var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return null;
                var application = await _context.Applications.Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).Where(f => f.Applicant.UserId == user.UserId && f.ApplicationStatus.Status.ToLower() != ApplicationStatusEnum.Incomplete.GetDescription().ToLower()).ToListAsync();

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<List<Applications>> GetIncompleteApplicationsByUserName(int userId)
        {
            try
            {
                if (userId == 0) return null;
                var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return null;
                var application = await _context.Applications.Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).Where(f => f.Applicant.UserId == user.UserId && f.ApplicationStatus.Status.ToLower() == ApplicationStatusEnum.Incomplete.GetDescription().ToLower()).ToListAsync();

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<Applications> Update(ApplicationDetailsViewModel model)
        {
            try
            {
                UpdateWorkFlowRefId();

                if (model.Id == 0) return null;
                var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
                if (user == null)
                {
                    user = new User();
                    user.UserId = model.UserId;
                }
                var fundingCall = await _context.FundingCalls.FirstOrDefaultAsync(f => f.Id == model.FundingCallDetailsId);
                var end = new DateTime(0001, 01, 01);


                var executiveManagementDesc = EmployeePersonTypeEnum.ExecutiveManagement.GetDescription().Trim().ToLower();
                var viceDeanDesc = EmployeePersonTypeEnum.ViceDean.GetDescription().Trim().ToLower();
                var directorDesc = EmployeePersonTypeEnum.Director.GetDescription().Trim().ToLower();

                // Normalize input strings

                var hodPersonType = user?.HodPersonType?.Trim().ToLower();
                var viceDeanPersonType = user?.ViceDeanPersonType?.Trim().ToLower();
                var position = user?.Position?.Trim().ToLower();


                if (entity == null) return null;
                {
                    if (model.LastSavedStep.ToLower().Trim() == ApplicationStepsEnum.ApplicantDetails.GetDescription().ToLower().Trim())
                    {
                        var status = await _context.ApplicationStatus.FirstOrDefaultAsync(s => s.Status.ToLower() == ApplicationStatusEnum.Incomplete.GetDescription().ToLower());

                        entity.Applicant = user;
                        entity.FundingCalls = fundingCall;
                        entity.ApplicationStatus = status;
                        entity.LastSavedStep = model.LastSavedStep;
                        entity.ApplicationStartDate = model.StartDate;
                        entity.ApplicantCategory = model.ApplicantCategory;
                        entity.AppointmentCategory = model.AppointmentCategory;
                        entity.FundingStartDate = model.FundingStartDate == end ? entity.FundingStartDate : model.FundingStartDate;
                        entity.FundingEndDate = model.FundingEndDate == end ? entity.FundingEndDate : model.FundingEndDate;
                        entity.CostCentreName = model.CostCentreName;
                        entity.CostCentreNumber = model.CostCentreNumber;

                        entity.PreviousFundingYear = model.PreviousFundingYear;
                        entity.PreviousFundingAmount = model.PreviousFundingAmount;
                        entity.PreviousFundingOutcome = model.PreviousFundingOutcome;
                        entity.LastModifiedDate = DateTime.Now;

                        await _context.SaveChangesAsync();
                    }

                    if (model.LastSavedStep.ToLower().Trim() == ApplicationStepsEnum.ApproveProjects.GetDescription().ToLower().Trim())
                    {
                        entity.LastSavedStep = model.LastSavedStep;
                        entity.Applicant = user;
                        var applicationsProjects = _context.ApplicationsProjects.Where(c => c.ApplicationsId == entity.Id).ToList();

                        foreach (var items in applicationsProjects)
                        {
                            _context.ApplicationsProjects.Remove(items);
                            await _context.SaveChangesAsync();
                        }
                        if (model.ProjectId?.Any() == true)
                        {
                            foreach (var projectId in model.ProjectId)
                            {
                                _context.AddRange(new ApplicationsProjects { ProjectsId = Convert.ToInt32(projectId), ApplicationsId = model.Id });
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    if (model.LastSavedStep.ToLower().Trim() == ApplicationStepsEnum.ImprovementOfStaffQualifications.GetDescription().ToLower().Trim())
                    {
                        entity.LastSavedStep = model.LastSavedStep;
                        entity.StudyingTowards = model.StudyingTowards;
                        entity.PlannedGraduationYear = model.PlannedGraduationYear;
                        entity.FirstYearRegistration = model.FirstYearRegistration;
                        if (model.AppointmentOption != null)
                            entity.AppointmentOption = model.AppointmentOption[0] != null
                                ? model.AppointmentOption[0]
                                : "";
                        entity.Describe = model.Describe;
                        if (model.SupportRequired != null)
                            entity.SupportRequired = model.SupportRequired[0] != null ? model.SupportRequired[0] : "";
                        entity.AppointmentDescribe = model.AppointmentDescribe;

                        await _context.SaveChangesAsync();

                        var supportRequiredList = new List<string>();
                        if (model.SupportRequired != null)
                            foreach (var s in model.SupportRequired)
                            {
                                var supportList = s.Split(",");
                                foreach (var prj in supportList)
                                {
                                    if (!supportRequiredList.Contains(prj))
                                    {
                                        supportRequiredList.Add(prj);
                                    }
                                }
                            }


                        if (supportRequiredList?.Any() == true)

                        {
                            var supportReqList = await _context.ApplicationSupportRequired.Where(s => s.ApplicationsId == model.Id).ToListAsync();

                            foreach (var items in supportReqList)
                            {
                                _context.ApplicationSupportRequired.Remove(items);
                                await _context.SaveChangesAsync();
                            }


                            foreach (var item in supportRequiredList)
                            {
                                var reqList = await _context.SupportRequired.FirstOrDefaultAsync(s => s.Description.ToLower().Trim() == item.ToLower().Trim());

                                if (reqList == null) continue;
                                if (reqList.Id != 0)
                                {
                                    _context.AddRange(new ApplicationSupportRequired { SupportRequiredId = Convert.ToInt32(reqList.Id), ApplicationsId = model.Id });
                                }
                            }
                        }

                        await _context.SaveChangesAsync();

                    }

                    if (model.LastSavedStep.ToLower().Trim() == ApplicationStepsEnum.ImproveStaffResearch.GetDescription().ToLower().Trim())
                    {
                        entity.LastSavedStep = model.LastSavedStep;
                        entity.FinancialSupport = model.FinancialSupport[0] != null ? model.FinancialSupport[0] : "";
                        entity.Applicant = user;

                        await _context.SaveChangesAsync();
                    }

                    if (model.LastSavedStep.ToLower().Trim() == ApplicationStepsEnum.SupportingDocuments.GetDescription().ToLower().Trim())
                    {
                        entity.LastSavedStep = model.LastSavedStep;

                        await _context.SaveChangesAsync();
                    }

                    if (model.LastSavedStep.ToLower().Trim() == ApplicationStepsEnum.CareerDevelopment.GetDescription().ToLower().Trim())
                    {
                        entity.LastSavedStep = model.LastSavedStep;
                        entity.CareerFinancialSupport = model.CareerFinancialSupport[0] != null ? model.CareerFinancialSupport[0] : "";
                        entity.CareerTeachingRelief = model.CareerTeachingRelief[0] != null ? model.CareerTeachingRelief[0] : "";

                        await _context.SaveChangesAsync();

                        var careerTeachingReliefList = new List<string>();
                        if (model.CareerTeachingRelief[0] != null)
                        {
                            foreach (var s in model.CareerTeachingRelief)
                            {
                                var teachingList = s.Split(",");
                                foreach (var prj in teachingList)
                                {
                                    if (!careerTeachingReliefList.Contains(prj))
                                    {
                                        careerTeachingReliefList.Add(prj);
                                    }
                                }
                            }

                            if (careerTeachingReliefList?.Any() == true)
                            {
                                var supportReqList = await _context.ApplicationSupportRequired.Where(s => s.ApplicationsId == model.Id).ToListAsync();

                                foreach (var items in supportReqList)
                                {
                                    _context.ApplicationSupportRequired.Remove(items);
                                    await _context.SaveChangesAsync();
                                }


                                foreach (var item in careerTeachingReliefList)
                                {
                                    var reqList = await _context.SupportRequired.FirstOrDefaultAsync(s => s.Description.ToLower().Trim() == item.ToLower().Trim());

                                    if (reqList == null) continue;
                                    if (reqList.Id != 0)
                                    {
                                        _context.AddRange(new ApplicationSupportRequired { SupportRequiredId = Convert.ToInt32(reqList.Id), ApplicationsId = model.Id });
                                    }
                                }
                            }

                            await _context.SaveChangesAsync();
                        }
                        var careerFinancialSupportList = new List<string>();

                        if (model.CareerFinancialSupport[0] != null)
                        {
                            foreach (var u in model.CareerFinancialSupport)
                            {
                                var teachingList = u.Split(",");
                                foreach (var prj in teachingList)
                                {
                                    if (!careerFinancialSupportList.Contains(prj))
                                    {
                                        careerFinancialSupportList.Add(prj);
                                    }
                                }
                            }

                            if (careerFinancialSupportList?.Any() == true)
                            {
                                var supportReqList = await _context.ApplicationSupportRequired.Where(s => s.ApplicationsId == model.Id).ToListAsync();

                                foreach (var items in supportReqList)
                                {
                                    _context.ApplicationSupportRequired.Remove(items);
                                    await _context.SaveChangesAsync();
                                }


                                foreach (var item in careerFinancialSupportList)
                                {
                                    var reqList = await _context.SupportRequired.FirstOrDefaultAsync(s => s.Description.ToLower().Trim() == item.ToLower().Trim());

                                    if (reqList == null) continue;
                                    if (reqList.Id != 0)
                                    {
                                        _context.AddRange(new ApplicationSupportRequired { SupportRequiredId = Convert.ToInt32(reqList.Id), ApplicationsId = model.Id });
                                    }
                                }
                            }

                            await _context.SaveChangesAsync();
                        }
                    }

                    if (model.LastSavedStep.ToLower().Trim() !=
                        ApplicationStepsEnum.FinalStep.GetDescription().ToLower().Trim()) return entity;
                    ApplicationStatus finalStatus = new ApplicationStatus();
                    {
                        _workFlowIntegration.Options = _options;
                        _workFlowIntegration.WorkflowDefinitionName = _options.WorkflowDefinitionName;

                        var workflowInstanceResource = new WorkflowInstanceResource();
                        var res = _workFlowIntegration.IsWorkFlowSetUp();
                        if (res != null)
                        {

                            var workFlowInst = new CreateWorkflowInstanceResource
                            {
                                CreatedBy = entity.Applicant.Username,
                                WorkflowDefinitionId = res.WorkflowDefinitionId
                            };
                            workflowInstanceResource = await _workFlowIntegration.CreateWorkflowInstance(workFlowInst);
                        }

                        var sendFundAdminEmail = false;
                        if (entity.RFIByFundAdmin == "Yes")
                        {
                            if (entity.DHETFundsRequested == model.DHETFundsRequested || float.Parse(entity.DHETFundsRequested, CultureInfo.InvariantCulture.NumberFormat) > float.Parse(model.DHETFundsRequested, CultureInfo.InvariantCulture.NumberFormat))
                            {
                                finalStatus = await _context.ApplicationStatus.FirstOrDefaultAsync(s => s.Status.ToLower() == ApplicationStatusEnum.ApprovedbyViceDean.GetDescription().ToLower());
                                sendFundAdminEmail = true;
                            }
                        }
                        else
                        {
                            finalStatus = await _context.ApplicationStatus.FirstOrDefaultAsync(s => s.Status.ToLower() == ApplicationStatusEnum.PendingApproval.GetDescription().ToLower());

                        }


                        if (await IsHodMec(user, entity))
                        {
                            finalStatus = await _context.ApplicationStatus.FirstOrDefaultAsync(s =>
                                s.Status.Trim().ToLower() == ApplicationStatusEnum.PendingApprovalByFa.GetDescription().Trim().ToLower());
                        }
                        else
                        {
                            finalStatus = await _context.ApplicationStatus.FirstOrDefaultAsync(s =>
                                s.Status.Trim().ToLower() == ApplicationStatusEnum.PendingApprovalByHod.GetDescription().Trim().ToLower());

                        }


                        if (entity.ApplicationStatus.Status == ApplicationStatusEnum.ApprovedbyHOD.GetDescription().Trim().ToLower())
                        {

                            if (await IsViceDeanMec(user, entity))
                            {
                                finalStatus = await _context.ApplicationStatus.FirstOrDefaultAsync(s =>
                                    s.Status.Trim().ToLower() == ApplicationStatusEnum.PendingApprovalByFa.GetDescription().Trim().ToLower());
                            }
                            else
                            {
                                finalStatus = await _context.ApplicationStatus.FirstOrDefaultAsync(s =>
                                    s.Status.Trim().ToLower() == ApplicationStatusEnum.PendingApprovalByHod.GetDescription().Trim().ToLower());

                            }
                        }


                        entity.LastSavedStep = model.LastSavedStep;
                        entity.FinancialMotivation = model.FinancialMotivation;
                        entity.ApplicantProgress = model.ApplicantProgress;
                        entity.OutputMeasure = model.OutputMeasure;
                        entity.FacultyContibution =
                            model.FacultyContibution == ".00" ? "0.00" : model.FacultyContibution;
                        entity.DepartmentContribution = model.DepartmentContribution == ".00"
                            ? "0.00"
                            : model.DepartmentContribution;
                        entity.ResearchFundsContribution = model.ResearchFundsContribution == ".00"
                            ? "0.00"
                            : model.ResearchFundsContribution;
                        entity.DHETFundsRequested = model.DHETFundsRequested;
                        entity.TotalCost = model.TotalCost;
                        entity.ReferenceNumber = model.ReferenceNumber;
                        entity.ApplicationEndDate = DateTime.Now;
                        entity.ApplicationStatus = finalStatus;
                        entity.ReferenceId = workflowInstanceResource?.Id ?? Guid.Empty;
                        entity.RFIByFundAdmin = "No";
                        entity.SubmittedDate = DateTime.Now;
                        entity.LastModifiedDate = DateTime.Now;
                        entity.OtherFunding = model.OtherFunding == ".00" ? "0.00" : model.OtherFunding;


                        await _context.SaveChangesAsync();

                        if (workflowInstanceResource == null)
                            _ = UpdateWorkflowReferenceId(entity.Id);

                        var lstApprover = new List<ApplicationApproversViewModel>();

                        if (finalStatus.Status.ToLower() == ApplicationStatusEnum.PendingApprovalByFa.GetDescription().ToLower())
                        {
                            _emailSentRepository.SendNotificationEmail(model, user, entity, "Applicant",
                                "New Application Submitted - Applicant", "NewApplicationSubmitted", lstApprover,
                                "");
                            _emailSentRepository.SendNotificationEmail(model, user, entity, "Fund Administrator",
                                "New Application Received - Fund Administrator ", "ApplicationFundingAdminReceivedEmail",
                                GetApproverDetailsByStaffNo(entity,
                                    AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim(),
                                    model.LastModifierUsername), "");
                        }
                        else
                        {


                            //  if (sendFundAdminEmail == false)
                            //   {
                            _emailSentRepository.SendNotificationEmail(model, user, entity, "Applicant",
                                "New Application Submitted - Applicant", "NewApplicationSubmitted", lstApprover,
                                "");
                            _emailSentRepository.SendNotificationEmail(model, user, entity, "HOD",
                                "New Application Received - HOD ", "NewApplicationHODReceived",
                                GetApproverDetailsByStaffNo(entity,
                                    AssignableUserRoles.HOD.GetDescription().ToLower().Trim(),
                                    model.LastModifierUsername), "");
                            //    }
                            //else
                            //{
                            //    _emailSentRepository.SendNotificationEmail(model, user, entity, "HOD",
                            //        "New Application Received - HOD ", "NewApplicationHODReceived",
                            //        GetApproverDetailsByStaffNo(entity,
                            //            AssignableUserRoles.HOD.GetDescription().ToLower().Trim(),
                            //            model.LastModifierUsername), "");
                            //}
                        }
                    }


                }

                return entity;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<Applications> UpdateAmountApproved(UpdateApplicationStatusResource statusUpdateModel)
        {
            try
            {
                if (statusUpdateModel.ApplicationId == 0) return null;
                var application = _context.Applications.Include(c => c.FundingCalls).Include(c => c.Applicant).Include(c => c.ApplicationStatus).FirstOrDefault(u => u.Id == statusUpdateModel.ApplicationId);
                var userName = _userStoreDbContext.Users.FirstOrDefault(u => u.Username.ToLower().Trim() == statusUpdateModel.CurrentUsername.ToLower().Trim());

                if (userName != null)
                {
                    var fullName = userName.Name + " " + userName.Surname;


                    if (application == null) return null;
                    application.ApprovedAmount = statusUpdateModel.ApprovedAmount;
                    application.FundAdminApprovedAmount = statusUpdateModel.FundAdminApprovedAmount;
                    application.LastModifierUsername = fullName;
                }

                if (application == null) return application;
                application.FundAdminComment = statusUpdateModel.FundAdminComment;
                application.RFIByFundAdmin = statusUpdateModel.StatusName.ToLower() == ApplicationStatuesConstants.ReturnedRorInfo.ToLower()
                        ? "Yes"
                        : "No";
                await _context.SaveChangesAsync();
                return application;
            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }




        public async Task<Applications> UpdateSiaAmountApproved(UpdateApplicationStatusResource statusUpdateModel)
        {
            try
            {
                if (statusUpdateModel.ApplicationId == 0) return null;
                var application = _context.Applications.Include(c => c.FundingCalls).Include(c => c.Applicant).Include(c => c.ApplicationStatus).FirstOrDefault(u => u.Id == statusUpdateModel.ApplicationId);
                var userName = _userStoreDbContext.Users.FirstOrDefault(u => u.Username.ToLower().Trim() == statusUpdateModel.CurrentUsername.ToLower().Trim());
                if (userName != null)
                {
                    var fullName = userName.Name + " " + userName.Surname;

                    if (application != null)
                    {
                        application.ApprovedAmount = statusUpdateModel.ApprovedAmount;
                        application.LastModifierUsername = fullName;
                        application.SIAComment = statusUpdateModel.SIAComment;
                        application.FundAdminApprovedAmount = statusUpdateModel.StatusName == "Returned for Info" ? "0.00" : application.FundAdminApprovedAmount;
                        application.FundAdminComment = statusUpdateModel.StatusName == "Returned for Info" ? null : application.FundAdminComment;

                        await _context.SaveChangesAsync();
                    }
                }

                var model = new ApplicationDetailsViewModel();

                if (application == null) return null;
                model.Id = application.Id;
                model.ApprovedAmount = application.ApprovedAmount;
                model.FundAdminApprovedAmount = application.FundAdminApprovedAmount;
                model.FundAdminComment = application.FundAdminComment;
                model.ReferenceNumber = application.ReferenceNumber;
                model.SIAComment = application.SIAComment;
                model.FundingCallDetailsId = application.FundingCalls.Id;

                // Send Award Letter Ready email when SIA Director approves
                if (statusUpdateModel.StatusName?.ToLower() == "approved")
                {
                    try
                    {
                        var emailModel = new ApplicationDetailsViewModel
                        {
                            Id = application.Id,
                            ReferenceNumber = application.ReferenceNumber,
                            UserId = application.Applicant?.UserId ?? 0,
                            FundingCallDetailsId = application.FundingCalls?.Id ?? 0,
                            ApprovedAmount = application.ApprovedAmount,
                            FundAdminApprovedAmount = application.FundAdminApprovedAmount
                        };

                        await _emailSentRepository.SendAwardLetterReadyEmail(emailModel);
                    }
                    catch (Exception emailEx)
                    {
                        SaveErrorLog("UpdateSiaAmountApproved", "Error sending award letter ready email: " + emailEx.Message, application.ReferenceNumber);
                    }
                }
                return application;
            }
            catch (Exception msg)
            {
                throw new NotImplementedException("Error: " + msg.ToString());
            }
        }

        public async Task<List<Applications>> GetAllApplicationsByUserId(int userId)
        {
            try
            {
                if (userId == 0) return null;
                var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return null;
                var application = await _context.Applications.Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).Where(f => f.Applicant.UserId == user.UserId && f.ApplicationStatus.Status.ToLower() != ApplicationStatusEnum.Incomplete.GetDescription().ToLower() && f.ReferenceNumber != null).ToListAsync();
                return application;
            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<List<Applications>> FilterGetMyApplications(SearchApplicationViewModel model)
        {
            var res = await _context.Applications
                .Include(o => o.ApplicationStatus)
                .Include(o => o.Applicant)
                .Include(o => o.FundingCalls)
                .Where(o => o.Applicant.UserId == model.UserId).ToListAsync();

            if (!string.IsNullOrEmpty(model.FundingCallName))
            {
                res = res.Where(x => x.FundingCalls.FundingCallName.ToLower().Contains(model.FundingCallName.ToLower())).ToList();

            }

            if (model.StatusId != 0)
            {
                res = res.Where(x => x.ApplicationStatus.ApplicationStatusId == model.StatusId).ToList();

            }

            if (!string.IsNullOrEmpty(model.RefferenceNumber))
            {
                res = res.Where(x => x.ReferenceNumber == model.RefferenceNumber).ToList();

            }
            foreach (var application in res)
            {
                var fundingCall = application.FundingCalls;


                if (fundingCall != null)
                {
                    var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                    .ToListAsync();


                    fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                }
            }
            return res;
        }

        public async Task<List<Applications>> GetNewApplications()
        {
            var res = await _context.Applications
                .Include(o => o.ApplicationStatus)
                .Include(o => o.Applicant)
                .Include(o => o.FundingCalls)
                .Where(o => o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.PendingApproval.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).ToListAsync();
            return res;
        }

        private async Task<List<Projects>> GetApplicationProjectsByApplicationId(int applicationId)
        {

            var allProjects = await _context.Projects.Select(p => new { p.Id, p.ProjectName }).ToListAsync();

            var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == applicationId)
                                       .ToListAsync();

            var lstPrj = new List<Projects>();

            if (applicationId == 3484)
            {
                int i = 0;
            }
            try
            {
                if (appProjects.Count > 0)
                {
                    var sb = new StringBuilder();
                    // Corrected type


                    foreach (var appProject in appProjects)
                    {

                        var ProjectDetails = allProjects.FirstOrDefault(p => p.Id == appProject.ProjectsId);


                        if (ProjectDetails != null)
                        {

                            var prj = new Projects();
                            prj.ProjectName = ProjectDetails.ProjectName;
                            prj.Id = ProjectDetails.Id;

                            lstPrj.Add(prj);
                        }
                    }
                }

                return lstPrj;
            }
            catch (Exception ex)
            {

                return lstPrj;
            }
        }

        public async Task<List<Applications>> GetNewFundingAdminApplications()
        {
            var allProjects = await _context.Projects.Select(p => new { p.Id, p.ProjectName }).ToListAsync();


            var res = await _context.Applications
                .Include(o => o.ApplicationStatus)
                .Include(o => o.Applicant)
                .Include(o => o.FundingCalls)
                .OrderByDescending(c => c.LastModifiedDate).ThenBy(c => c.ApplicationEndDate).Where(o => o.ReferenceNumber != null).ToListAsync();

            //return res;

            List<Applications> updatedApplications = new List<Applications>();



            foreach (var application in res)
            {

                var fundingCalls = application.FundingCalls;

                if (fundingCalls != null)
                {
                    // Fetch new projects assigned to the current application
                    var newProjects = await GetApplicationProjectsByApplicationId(application.Id);

                    if (application.ReferenceNumber == "202500082")
                    {
                        int aa = 0; // Debugging
                    }


                    // Ensure FundingCallProjects is initialized for the current FundingCall
                    if (fundingCalls.FundingCallProjects == null)
                    {
                        fundingCalls.FundingCallProjects = new List<Projects>();
                    }
                    else
                    {
                        // Clear the old projects (if needed)
                        fundingCalls.FundingCallProjects.Clear();
                    }

                    // Add only the new projects for this application
                    //fundingCalls.FundingCallProjects.AddRange(newProjects);


                    //Create a new instance of the application to avoid reference issues
                    var updatedApplication = new Applications
                    {
                        Id = application.Id,
                        ApplicationStatus = application.ApplicationStatus,
                        Applicant = application.Applicant,
                        FundingCalls = new FundingCalls
                        {
                            Id = application.FundingCalls.Id,
                            FundingCallName = application.FundingCalls.FundingCallName,
                            FundingBudget = application.FundingCalls.FundingBudget,
                            ShortDescription = application.FundingCalls.ShortDescription,
                            OpeningDate = application.FundingCalls.OpeningDate,
                            ClosingDate = application.FundingCalls.ClosingDate,
                            AmendedClosingDate = application.FundingCalls.AmendedClosingDate,
                            CreatedDate = application.FundingCalls.CreatedDate,
                            CreatedBy = application.FundingCalls.CreatedBy,
                            ModifiedBy = application.FundingCalls.ModifiedBy,
                            ModifiedDate = application.FundingCalls.ModifiedDate,
                            FundingCallProjects = newProjects
                        },
                        FundingStartDate = application.FundingStartDate,
                        FundingEndDate = application.FundingEndDate,
                        ApplicantCategory = application.ApplicantCategory,
                        ApplicantProgress = application.ApplicantProgress,
                        AppointmentCategory = application.AppointmentCategory,
                        LastSavedStep = application.LastSavedStep,

                        //Improvement of Staff Qualifications Step
                        StudyingTowards = application.StudyingTowards,
                        FirstYearRegistration = application.FirstYearRegistration,
                        PlannedGraduationYear = application.PlannedGraduationYear,
                        Describe = application.Describe,
                        AppointmentDescribe = application.AppointmentDescribe,
                        SupportRequired = application.SupportRequired,
                        AppointmentOption = application.SupportRequired,
                        FinancialSupport = application.FinancialSupport,
                        CareerFinancialSupport = application.FinancialSupport,
                        CareerTeachingRelief = application.CareerTeachingRelief,

                        //Final Step
                        FinancialMotivation = application.FinancialMotivation,
                        OutputMeasure = application.OutputMeasure,
                        FacultyContibution = application.FacultyContibution,
                        DepartmentContribution = application.DepartmentContribution,
                        OtherFunding = application.OtherFunding,
                        ResearchFundsContribution = application.ResearchFundsContribution,
                        DHETFundsRequested = application.DHETFundsRequested,
                        TotalCost = application.TotalCost,
                        ApprovedAmount = application.ApprovedAmount,
                        FundAdminApprovedAmount = application.FundAdminApprovedAmount,
                        FundAdminComment = application.FundAdminComment,
                        SIAComment = application.SIAComment,
                        ReferenceNumber = application.ReferenceNumber,
                        ReferenceId = application.ReferenceId,
                        LastModifiedDate = application.LastModifiedDate,
                        LastModifierUsername = application.LastModifierUsername,
                        SubmittedDate = application.SubmittedDate,

                        CostCentreName = application.CostCentreName,
                        CostCentreNumber = application.CostCentreNumber,
                        PreviousFundingYear = application.PreviousFundingYear,
                        PreviousFundingOutcome = application.PreviousFundingOutcome,
                        PreviousFundingAmount = application.PreviousFundingAmount,
                        RFIByFundAdmin = application.RFIByFundAdmin,
                        ApplicationEndDate = application.ApplicationEndDate,
                        ApplicationStartDate = application.ApplicationStartDate
                    };


                    updatedApplications.Add(updatedApplication);





                }

            }


            return updatedApplications;

        }

        public async Task<List<Applications>> GetFundingAdminApplications(int applicationStatusId)
        {
            var res = await _context.Applications.Where(c => c.ApplicationStatus.ApplicationStatusId == applicationStatusId)
                 .Include(o => o.ApplicationStatus)
                 .Include(o => o.Applicant)
                 .Include(o => o.FundingCalls).OrderByDescending(c => c.LastModifiedDate).ThenBy(c => c.ApplicationEndDate).Where(o => o.ReferenceNumber != null).ToListAsync();

            foreach (var application in res)
            {
                var fundingCall = application.FundingCalls;


                if (fundingCall != null)
                {
                    var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                    .ToListAsync();
                    fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                }
            }

            return res;
        }

        public async Task<List<Applications>> GetNewSiaDirectorApplications()
        {
            var res = await _context.Applications
                    .Include(o => o.ApplicationStatus)
                    .Include(o => o.Applicant)
                    .Include(o => o.FundingCalls)
                    .Where(o => o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbyFundAdmin.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).ToListAsync();


            foreach (var application in res)
            {
                var fundingCall = application.FundingCalls;


                if (fundingCall != null)
                {
                    var appProjects = await _context.ApplicationsProjects.Where(fc => fc.ApplicationsId == application.Id)
                                    .ToListAsync();
                    fundingCall.FundingCallProjects = await GetApplicationProjectsByApplicationId(application.Id);

                }
            }

            return res;
        }

        public async Task<List<Applications>> GetNewFbpApplications()
        {
            var res = await _context.Applications
                 .Include(o => o.ApplicationStatus)
                 .Include(o => o.Applicant)
                 .Include(o => o.FundingCalls)
                 .Where(o => o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbySIADirector.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).ToListAsync();
            return res;
        }

        public async Task<List<Applications>> GetProcessedApplications()
        {
            var res = await _context.Applications
                .Include(o => o.ApplicationStatus)
                .Include(o => o.Applicant)
                .Include(o => o.FundingCalls)
                .Where(o => o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.Approved.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).ToListAsync();
            return res;
        }

        public async Task<List<Applications>> GetDvcNewApplications(int userId)
        {
            try
            {
                var applications = new List<Applications>();

                var dvcDetails = await _userStoreDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(dvcDetails.HRPostNumber)) return applications;
                {
                    var dvcUsers = await _userStoreDbContext.Users.Where(u => u.ViceDeanStaffNumber.ToLower().Trim() == dvcDetails.HRPostNumber.ToLower().Trim()).ToListAsync();

                    foreach (var user in dvcUsers)
                    {
                        var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.Applicant.UserId == user.UserId && o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.PendingDVCApproval.GetDescription().ToLower().Trim()).Where(o => o.ReferenceNumber != null).ToListAsync();

                        applications.AddRange(res);
                    }
                }

                return applications;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }

        }

        public async Task<List<Applications>> GetDvcProcessedApplications(int userId)
        {
            try
            {
                var applications = new List<Applications>();

                var dvcDetails = await _userStoreDbContext.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(dvcDetails.HRPostNumber)) return applications;
                {
                    var dvcUsers = await _userStoreDbContext.Users.Where(u => u.ViceDeanStaffNumber.ToLower().Trim() == dvcDetails.HRPostNumber.ToLower().Trim()).ToListAsync();

                    foreach (var user in dvcUsers)
                    {
                        var res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.Applicant.UserId == user.UserId && o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.DVCApproved.GetDescription().ToLower().Trim()).ToListAsync();

                        applications.AddRange(res);
                    }
                }

                return applications;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<List<Comments>> GetAllComment()
        {
            try
            {
                var applicationStatuses = await _context.Comments.ToListAsync();

                return applicationStatuses.ToList();
            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        //public async Task<List<Comments>> GetCommentsByApplicationId(int applicationId)
        //{
        //    if (applicationId == 0) return null;
        //    var comments = await _context.Comments.Include(c => c.Applications).Include(c => c.Applications.FundingCalls).Include(c => c.User).Where(f => f.ApplicationsId == applicationId).ToListAsync();
        //    return comments;

        //}

        public async Task<List<Comments>> GetCommentsByApplicationId(int applicationId)
        {
            if (applicationId == 0) return new List<Comments>();

            // 1) Get comments from UCDG
            var comments = await _context.Comments
                .Where(c => c.ApplicationsId == applicationId)
                .Include(c => c.Applications)
                    .ThenInclude(a => a.FundingCalls)
                .Include(c => c.User) // legacy join only
                .AsNoTracking()
                .ToListAsync();

            // 2) Collect UserStore ids
            var usIds = comments
                .Where(c => c.UserStoreUserId != null)
                .Select(c => c.UserStoreUserId.Value)
                .Distinct()
                .ToList();

            // 3) Fetch users from UserStore
            var usUsers = usIds.Count == 0
                ? new Dictionary<int, UserStoreUser>()
                : await _userStoreDbContext.Users
                    .Where(u => usIds.Contains(u.UserId))
                    .AsNoTracking()
                    .ToDictionaryAsync(u => u.UserId);

            // 4) Build VM: prefer UserStore, fallback to UCDG
            return comments.Select(c =>
            {
                if (c.UserStoreUserId != null && usUsers.TryGetValue(c.UserStoreUserId.Value, out var us))
                {
                    return new Comments
                    {
                        Id = c.Id,
                        ApplicationsId = c.ApplicationsId,
                        Comment = c.Comment,
                        UserId = c.UserId,
                        UserStoreUserId = c.UserStoreUserId,
                        Username = us.Username,
                        DisplayName = $"{us.Name ?? ""} {us.Surname ?? ""}".Trim()
                    };
                }

                // fallback legacy
                return new Comments
                {
                    Id = c.Id,
                    ApplicationsId = c.ApplicationsId,
                    Comment = c.Comment,
                    UserId = c.UserId,
                    UserStoreUserId = c.UserStoreUserId,
                    Username = c.User?.Username,
                    DisplayName = $"{c.User?.FirstName ?? ""} {c.User?.Surname ?? ""}".Trim()
                };
            }).ToList();
        }

        public async Task<List<Applications>> GetFaCommentsByApplicationId(int applicationId)
        {
            if (applicationId == 0) return null;
            var comments = await _context.Applications.Include(c => c.FundingCalls).Include(c => c.Applicant).Where(f => f.Id == applicationId).ToListAsync();
            return comments;

        }

        public async Task<Comments> AddComments(AppplicationCommentVeiwModel model)
        {
            var comments = new Comments();
            try
            {
                var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
                if (user == null) return null;
                comments.UserId = user.UserId;
                comments.ApplicationsId = model.ApplicationsId;
                comments.Comment = model.Comment;

                await _context.Comments.AddAsync(comments);

                await _context.SaveChangesAsync();

                return comments;
            }

            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<List<Applications>> GetApprovalLetter(int userId)
        {
            var res = await _context.Applications
                .Include(o => o.ApplicationStatus)

               .Include(o => o.FundingCalls)
               .Include(o => o.Applicant)
               .Include(o => o.ReferenceId)

               .Where(o => o.FundingCalls.Id == userId && o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.Approved.GetDescription().ToLower().Trim()).ToListAsync();
            return res;
        }

        public async Task<Applications> GetApplicationsById(int id)
        {
            try
            {
                if (id == 0) return null;
                var application = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == id);

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<Applications> UpdateApplicationStatus(Guid referenceId, int statusId)
        {
            var application = await _context.Applications.Include(applications => applications.ApplicationStatus).FirstOrDefaultAsync(o => o.ReferenceId == referenceId);
            if (application == null) return null;
            application.ApplicationStatus.ApplicationStatusId = statusId;
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<Applications> UpdateAwardDeclinedByApplicant(UpdateApplicationStatusResource statusUpdateModel)
        {
            try
            {
                if (statusUpdateModel.ApplicationId != 0)
                {
                    var status = await _context.ApplicationStatus.Where(c => c.StatusName.Trim().ToLower() == ApplicationStatusEnum.AwawrdLetterDeclined.GetDescription().Trim().ToLower()).FirstOrDefaultAsync();
                    var application = _context.Applications.FirstOrDefault(u => u.Id == statusUpdateModel.ApplicationId);

                    if (application != null)
                    {

                        application.LastModifierUsername = statusUpdateModel.CurrentUsername;
                        application.ApplicationStatus = status;
                        await _context.SaveChangesAsync();
                    }

                    if (application == null) return null;
                    {
                        var model = new ApplicationDetailsViewModel
                        {
                            Id = application.Id,
                            ReferenceNumber = application.ReferenceNumber
                        };


                        if (model.Id == 0) return application;
                        int[] roleId = { 5, 4, 6 };

                        var user = await _context.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role).Where(u => u.IsActive && roleId.Contains(u.UserRoles.FirstOrDefault().RoleId)).ToListAsync();
                        var entity = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == model.Id);
                        if (entity != null)
                        {
                            _emailSentRepository.SendNotificationEmail(model, user.FirstOrDefault(o => o.UserRoles.FirstOrDefault()!.RoleId == 5), entity, "SIA Director", "Application Declined By the Applicant - SIA", "ApplicatDecliningAwardLetter", GetApproverDetailsByStaffNo(entity, AssignableUserRoles.SIADirector.GetDescription().ToLower().Trim(), statusUpdateModel.CurrentUsername), "");
                            _emailSentRepository.SendNotificationEmail(model, user.FirstOrDefault(o => o.UserRoles.FirstOrDefault()!.RoleId == 6), entity, "Financial Business Partner", "Application Declined By the Applicant - FBP", "ApplicantFBPDecliningAwardLetter", GetApproverDetailsByStaffNo(entity, AssignableUserRoles.FinancialBusinessPartner.GetDescription().ToLower().Trim(), statusUpdateModel.CurrentUsername), "");
                            _emailSentRepository.SendNotificationEmail(model, user.FirstOrDefault(o => o.UserRoles.FirstOrDefault()!.RoleId == 4), entity, "Fund Administrator", "Application Declined By the Applicant - FA", "ApplicantFundAdminDecliningAwardLetter", GetApproverDetailsByStaffNo(entity, AssignableUserRoles.FundAdministrator.GetDescription().ToLower().Trim(), statusUpdateModel.CurrentUsername), "");
                        }
                    }

                    return application;
                }
                return null;
            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }



        public string UpdateAwardAcceptingByApplicant(UpdateApplicationStatusResource statusUpdateModel)
        {
            try
            {
                int[] roleId = [5, 4, 6, 2, 1, 3];

                if (statusUpdateModel.ApplicationId == 0) return null;
                var status = _context.ApplicationStatus.AsNoTracking().Where(c => c.StatusName.Trim().ToLower() == ApplicationStatusEnum.AwawrdLetterAccepted.GetDescription().Trim().ToLower()).FirstOrDefault();
                var application = _context.Applications.FirstOrDefault(u => u.Id == statusUpdateModel.ApplicationId);

                var user = _context.Users
                    .AsNoTracking()
                    .Where(u => u.IsActive)
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ToList()
                    .Where(u => u.UserRoles.Any(ur => roleId.Contains(ur.RoleId)))
                    .ToList();


                var entity = _context.Applications
                    .AsNoTracking()
                    .Include(c => c.Applicant)
                    .Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus)
                    .FirstOrDefault(f => f.Id == application.Id);


                if (application != null)
                {

                    application.LastModifierUsername = statusUpdateModel.CurrentUsername;
                    application.ApplicationStatus = status;
                    _context.SaveChanges();
                }

                if (application == null) return null;
                {
                    var model = new ApplicationDetailsViewModel
                    {
                        Id = application.Id,
                        ReferenceNumber = application.ReferenceNumber
                    };


                    SendMailToApprover(statusUpdateModel, model, user, entity);


                    SendMailToApplicantCc(statusUpdateModel, model, user, entity);

                }

                return "";
            }
            catch (Exception msg)
            {
                SaveErrorLog("UpdateAwardAcceptingByApplicant", "Error - Unable to Update Award Accepting By Applicant" + msg.Message, "");
                return null;
            }
        }

        public void SendAwardLetterAcceptingByApplicant(UpdateApplicationStatusResource statusUpdateModel)
        {
            try
            {
                //   int[] roleId = [5, 4, 6, 2, 1, 3];

                var application = _context.Applications.FirstOrDefault(u => u.Id == statusUpdateModel.ApplicationId);

                //var user = _context.Users
                //    .AsNoTracking()
                //    .Where(u => u.IsActive)
                //    .Include(u => u.UserRoles)
                //    .ThenInclude(ur => ur.Role)
                //    .ToList()
                //    .Where(u => u.UserRoles.Any(ur => roleId.Contains(ur.RoleId)))
                //    .ToList();


                var entity = _context.Applications
                    .AsNoTracking()
                    .Include(c => c.Applicant)
                    .Include(c => c.FundingCalls)
                    .Include(c => c.ApplicationStatus)
                    .FirstOrDefault(f => f.Id == application.Id);

                if (application == null)
                    return;

                var model = new ApplicationDetailsViewModel
                {
                    Id = application.Id,
                    ReferenceNumber = application.ReferenceNumber
                };
                var messageId = _emailSentRepository.GetMessageIdByReminderEmailSubmission(model, entity, "Applicant", "Progress Report, Overdue for Submission - Applicant", "SendApplicantProgressReportReminder");

                if (entity != null)
                {
                    var mrd = new MessageRecipientDetails
                    {
                        ApplicationId = entity.Id,
                        MessageRecipientId = messageId,
                        CreatedDate = DateTime.Now
                    };

                    _context.MessageRecipientDetails.Add(mrd);
                }

                _context.SaveChanges();
            }
            catch (Exception msg)
            {
                SaveErrorLog("SendAwardLetterAcceptingByApplicant", "Error - Unable to Send Award Letter Accepting By Applicant" + msg.Message, "");

            }
        }

        public void SendAwardLetterAcceptingByApplicantTemp()
        {
            try
            {

                var application = _context.Applications.Where(u => u.ApplicationStatus.ApplicationStatusId == 8 && u.FundingStartDate.Year == 2024).ToList();



                application.ForEach(o =>
                {



                    var entity = _context.Applications
                        .AsNoTracking()
                        .Include(c => c.Applicant)
                        .Include(c => c.FundingCalls)
                        .Include(c => c.ApplicationStatus)
                        .FirstOrDefault(f => f.Id == o.Id);


                    var model = new ApplicationDetailsViewModel
                    {
                        Id = o.Id,
                        ReferenceNumber = o.ReferenceNumber
                    };



                    var messageId = _emailSentRepository.GetMessageIdByReminderEmailSubmission(model, entity, "Applicant", "Progress Report, Overdue for Submission", "SendApplicantProgressReportReminder");

                    if (entity != null)
                    {
                        var mrd = new MessageRecipientDetails
                        {
                            ApplicationId = entity.Id,
                            MessageRecipientId = messageId,
                            CreatedDate = DateTime.Now
                        };

                        _context.MessageRecipientDetails.Add(mrd);
                    }

                    _context.SaveChanges();


                });
            }
            catch (Exception msg)
            {
                SaveErrorLog("SendAwardLetterAcceptingByApplicant", "Error - Unable to Send Award Letter Accepting By Applicant" + msg.Message, "");

            }
        }

        public void SendMailToApplicantCc(UpdateApplicationStatusResource statusUpdateModel, ApplicationDetailsViewModel model, List<User> user, Applications entity)
        {
            try
            {
                var getApproverByRoleName = new List<ApplicationApproversViewModel>();

                var userId = entity.Applicant.UserId;

                var userRole = user.FirstOrDefault(o => o.UserId == userId && o.IsActive);
                if (!CheckIfRoleIsMEC(userRole.StaffNumber))
                {
                    _emailSentRepository.SendNotificationEmail(model, userRole, entity, "Applicant", "Award Letter Accepted By the Applicant", "ApplicatAcceptingAwardLetter", getApproverByRoleName, null);
                }
            }
            catch (Exception ex)
            {
                SaveErrorLog("SendMailToApplicantCC", "Error - Unable to Send Mail To Applicant" + ex.Message, "");

            }
        }


        public bool CheckIfRoleIsMEC(string empNo)
        {
            try
            {
                return _context.MecMembers.Any(u => u.EmployeeNo == empNo);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SendMailToApprover(UpdateApplicationStatusResource statusUpdateModel, ApplicationDetailsViewModel model, List<User> user, Applications entity)
        {
            try
            {
                var roleMappings = new List<(int RoleId, string RoleName, string EmailSubject, string EmailTemplate)>
                    {
                        (2, "HOD", "Award Letter Accepted By Applicant - HOD", "ApplicantAcceptingAwardLetterHOD"),
                        (3, "Executive / Vice dean", "Award Letter Accepted By Applicant - Executive Director/ Vice Dean", "ApplicantAcceptingAwardLetterVC"),
                        (4, "Fund Administrator", "Award Letter Accepted By Applicant - Fund Admin", "ApplicantAcceptingAwardLetterFundAdmin"),
                        (5, "SIA Director", "Award Letter Accepted By Applicant - SIA Director", "ApplicantAcceptingAwardLetterSIA")
                        //,(6, "Financial Business Partner", "Award Letter Accepted By the Applicant - FBP", "ApplicantAcceptingAwardLetterFBP")
                    };

                var roleArray = new List<string> { "hod", "executive / vice dean", "fund administrator", "sia director", "financial business partner" };

                var approverList = GetApproverDetailsByStaffNoV2(entity, roleArray);

                foreach (var (roleId, roleName, emailSubject, emailTemplate) in roleMappings)
                {
                    var userRole = user.FirstOrDefault(o => o.UserRoles.FirstOrDefault().RoleId == roleId && o.IsActive);

                    var getApproverByRoleName = approverList.Where(a => a.RoleName.Contains(roleName.ToLower().Trim())).ToList();
                    var approverStaffNumber = getApproverByRoleName[0].StaffNumber;

                    if (!CheckIfRoleIsMEC(approverStaffNumber))
                    {
                        _emailSentRepository.SendNotificationEmail(model, userRole, entity, roleName, emailSubject, emailTemplate, getApproverByRoleName, "");
                    }
                }
            }
            catch (Exception ex)
            {
                SaveErrorLog("SendMailToApprover", "Error - Unable to Send Mail ToApprover" + ex.Message, "");
            }
        }


        public async Task SaveMessageRecipientDetails(int id, int applicationId)
        {
            try
            {
                var mrd = new MessageRecipientDetails
                {
                    ApplicationId = applicationId,
                    MessageRecipientId = id,
                    CreatedDate = DateTime.Now
                };

                await _context.MessageRecipientDetails.AddAsync(mrd);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SaveErrorLog("SaveMessageRecipientDetails", "Error: Unable to Save Message Recipient Details " + ex.Message, "");
            }

        }

        public async Task<string> UpdateWorkflowReferenceId(int applicationId)
        {


            try
            {
                if (applicationId == 0) return "00000000-0000-0000-0000-000000000000";
                var applicationDetails = await _context.Applications
                    .Include(c => c.Applicant)
                    .FirstOrDefaultAsync(c => c.Id == applicationId);

                if (applicationDetails == null) return "00000000-0000-0000-0000-000000000000";
                {
                    if (applicationDetails.Applicant == null)
                    {
                        return string.Empty;
                    }

                    var userDetails = await _userStoreDbContext.Users
                        .FirstOrDefaultAsync(c => c.UserId == applicationDetails.Applicant.UserId);

                    var user = new User();
                    if (userDetails == null)
                    {
                        user = await _context.Users
                        .FirstOrDefaultAsync(c => c.UserId == applicationDetails.Applicant.UserId);
                    }

                    _workFlowIntegration.Options = _options;
                    _workFlowIntegration.WorkflowDefinitionName = _options.WorkflowDefinitionName;
                    var workflowInfo = _workFlowIntegration.IsWorkFlowSetUp();

                    var res = _workFlowIntegration.IsWorkFlowSetUp();
                    if (res == null) return applicationDetails.ReferenceId.ToString();
                    var workFlowInst = new CreateWorkflowInstanceResource
                    {
                        CreatedBy = userDetails != null ? userDetails.Username : user.Username,
                        WorkflowDefinitionId = res.WorkflowDefinitionId
                    };

                    var workflowInstanceResource = await _workFlowIntegration.CreateWorkflowInstance(workFlowInst);

                    applicationDetails.ReferenceId = workflowInstanceResource?.Id ?? Guid.Empty;

                    await _context.SaveChangesAsync();
                    return applicationDetails.ReferenceId.ToString();
                }
            }
            catch (Exception ex)
            {
                SaveErrorLog("UpdateWorkflowReferenceId", "Error" + ex.Message, "");
                throw new Exception("An error occurred while updating the workflow reference ID.", ex);
            }
        }


        public void SaveErrorLog(string functionality, string applicationError, string parameterValue)
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


        public List<ApplicationApproversViewModel> GetApproverDetailsByStaffNo(Applications entity, string sendToValue, string currentUsername)
        {
            var lstResults = new List<ApplicationApproversViewModel>();

            if (entity == null || sendToValue == "Applicant")
                return lstResults;

            try
            {
                if (entity.Applicant.UserId == 0)
                    return lstResults;

                var approverDetails = GetApproverDetails(entity, sendToValue);
                if (approverDetails == null) return lstResults;


                var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                var fullTitle = title + " " + $" {approverDetails.FirstName} {approverDetails.Surname}";

                lstResults.Add(new ApplicationApproversViewModel
                {


                    EmailAddress = approverDetails.EmailAddress,
                    FullTittle = fullTitle + "",
                    //  RoleName = item,
                    ApproverUserId = approverDetails.UserId,
                    Fistname = approverDetails.FirstName,
                    StaffNumber = approverDetails.StaffNumber
                });

                return lstResults;
            }
            catch
            {
                return lstResults;
            }
        }


        public List<ApplicationApproversViewModel> GetApproverDetailsByStaffNoV2(Applications entity, List<string> rolesNames)
        {
            var lstResults = new List<ApplicationApproversViewModel>();
            try
            {
                if (rolesNames.Count <= 0) return lstResults;
                foreach (var item in rolesNames)
                {

                    if (entity.Applicant.UserId == 0)
                        return lstResults;

                    var approverDetails = GetApproverDetails(entity, item);
                    if (approverDetails == null) continue;

                    var title = char.ToUpper(approverDetails.Title[0]) + "" + approverDetails.Title.Substring(1).ToLower();

                    var fullTitle = title + " " + $"{approverDetails.FirstName} {approverDetails.Surname}";

                    lstResults.Add(new ApplicationApproversViewModel
                    {
                        EmailAddress = approverDetails.EmailAddress,
                        FullTittle = fullTitle + " ",
                        RoleName = item,
                        ApproverUserId = approverDetails.UserId,
                        Fistname = approverDetails.FirstName,
                        StaffNumber = approverDetails.StaffNumber
                    });
                }
                return lstResults;
            }
            catch (Exception)
            {
                return lstResults;
            }

        }


        private User GetApproverDetails(Applications entity, string sendToValue)
        {
            try
            {
                var approverDetails = new User();

                switch (sendToValue.ToLower().Trim())
                {
                    case "hod":
                        approverDetails = _context.Users
                            .Include(o => o.UserRoles)
                            .ThenInclude(o => o.Role)
                            .FirstOrDefault(u => u.StaffNumber.ToLower().Trim() == entity.Applicant.HODStaffNUmber.ToLower().Trim() && u.IsActive);
                        break;
                    case "executive / vice dean":
                        approverDetails = _context.Users
                            .Include(o => o.UserRoles)
                            .ThenInclude(o => o.Role)
                            .FirstOrDefault(u => u.StaffNumber.ToLower().Trim() == entity.Applicant.HODStaffNUmber.ToLower().Trim() && u.IsActive);
                        break;
                    case "fund administrator":
                    case "sia director":
                    case "financial business partner":
                        var roleName = sendToValue;
                        var role = _userStoreDbContext.Roles
                            .FirstOrDefault(r => r.Name.ToLower().Trim() == roleName.ToLower().Trim());
                        if (role != null)
                        {
                            var userRole = _userStoreDbContext.UserRoles
                                .FirstOrDefault(u => u.RoleId == role.RoleId && u.IsActive);
                            approverDetails = _context.Users
                                .Include(o => o.UserRoles)
                                .ThenInclude(o => o.Role)
                                .FirstOrDefault(u => u.UserId == userRole.UserId && u.IsActive);
                        }
                        break;
                }

                return approverDetails;
            }
            catch (Exception)
            {

                return null;
            }


        }




        public async Task<List<Applications>> GetMyPreviousApplications(int userId)
        {

            var ids2 = await _userIdResolver.ResolveUserIdsAsyncV2(userId);

            //var ids2 = await _userIdResolver.ResolveUserIdsAsync(userId); // e.g., [299, 3208]
            if (ids2 == null || ids2.Count == 0)
                return new List<Applications>();

            var idSet = new HashSet<int>(ids2);

            var query = _context.Applications
                .AsNoTracking()
                .Where(a => idSet.Contains(a.UserId.Value))
                .Include(a => a.ApplicationStatus)
                .Include(a => a.Applicant)
                .Include(a => a.FundingCalls)
                .OrderByDescending(a => a.ApplicationEndDate);


            var sql = query.ToQueryString();
            var res = await query.ToListAsync();

            foreach (var application in res)
            {
                var fundingCall = application.FundingCalls;
                if (fundingCall != null)
                {
                    // Example of awaited work:
                    var appProjects = await _context.ApplicationsProjects
                        .Where(fc => fc.ApplicationsId == application.Id)
                        .ToListAsync();

                    fundingCall.FundingCallProjects =
                        await GetApplicationProjectsByApplicationId(application.Id);
                }
            }

            return res.OrderByDescending(c => c.ApplicationEndDate).ToList();
        }



        //    public async Task<List<MotivationLetterReadModel>> CreateLinkUserMotivationLetter(List<MotivationLetterReadModel> model)
        //{

        //       foreach (var item in model) {

        //            var entity = new LinkUserMotivationLetter
        //            {
        //                UserId = (int)item.UserId,
        //                FundingCallId = (int)item.FundingCallId,
        //                DocumentId = item.DocumentId,
        //                 DocumentExtention = item.DocumentExtention, DocumentFile = item.DocumentFile,
        //                Filename = item.Filename, UploadType = item.UploadType, DateAdded = DateTime.UtcNow
        //            };

        //           // _context.Set<LinkUserMotivationLetter>().Add(entity);

        //              await _context.LinkUserMotivationLetter.AddAsync(entity);

        //        }

        //        await _context.SaveChangesAsync();


        //        var  savedDocs = await _context.LinkUserMotivationLetter
        //                .Where(u => u.Filename.ToLower().Trim() == model[0].Filename.ToLower().Trim() && u.UploadType.ToLower().Trim() == model[0].UploadType.ToLower().Trim())
        //                .Select(item => new MotivationLetterReadModel
        //                {
        //                    Id = item.Id,
        //                    Filename = item.Filename,
        //                    DocumentExtention = item.DocumentExtention,
        //                   // DocumentFile = item.DocumentFile,
        //                    UploadType = item.UploadType

        //                })
        //            .ToListAsync();

        //            return savedDocs;

        //        }



        //public async Task<List<MotivationLetterResponse>> CreateLinkUserMotivationLetter(List<MotivationLetterReadModel> model)
        // {
        //     if (model == null || !model.Any())
        //         return new List<MotivationLetterResponse>();

        //     var entities = model.Select(item => new LinkUserMotivationLetter
        //     {
        //         UserId = item.UserId ?? 0,
        //         FundingCallId = item.FundingCallId ?? 0,
        //         DocumentId = item.DocumentId,
        //         DocumentExtention = item.DocumentExtention,
        //         DocumentFile = item.DocumentFile,
        //         Filename = item.Filename,
        //         UploadType = item.UploadType,
        //         DateAdded = DateTime.UtcNow
        //     }).ToList();

        //     await _context.LinkUserMotivationLetter.AddRangeAsync(entities);
        //     await _context.SaveChangesAsync();

        //     var filename = model.First().Filename?.Trim().ToLower();
        //     var uploadType = model.First().UploadType?.Trim().ToLower();

        //     var savedDocs = await _context.LinkUserMotivationLetter
        //         .Where(u =>
        //             u.Filename.ToLower().Trim() == filename &&
        //             u.UploadType.ToLower().Trim() == uploadType)
        //         .Select(item => new MotivationLetterResponse
        //         {
        //             Id = item.Id,
        //             Filename = item.Filename,
        //             DocumentExtention = item.DocumentExtention,
        //             UploadType = item.UploadType
        //         })
        //         .ToListAsync();

        //     return savedDocs;
        // }


        public async Task<List<MotivationLetterResponse>> CreateLinkUserMotivationLetter(List<MotivationLetterReadModel> model)
        {
            if (model == null || model.Count == 0)
                return new List<MotivationLetterResponse>();

            var entities = model.Select(item => new LinkUserMotivationLetter
            {
                UserId = item.UserId ?? 0,
                FundingCallId = item.FundingCallId ?? 0,
                DocumentId = item.DocumentId,
                DocumentExtention = item.DocumentExtention,
                DocumentFile = item.DocumentFile,
                Filename = item.Filename,
                UploadType = item.UploadType,
                DateAdded = DateTime.UtcNow
            }).ToList();

            await _context.LinkUserMotivationLetter.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            var result = entities.Select(e => new MotivationLetterResponse
            {
                Id = e.Id,
                Filename = e.Filename,
                DocumentExtention = e.DocumentExtention,
                UploadType = e.UploadType,
                DateAdded = e.DateAdded
            }).ToList();

            return result;
        }

        public async Task<List<MotivationLetterReadModel>> CreateLinkUserMotivationLetterV2(List<MotivationLetterReadModel> model)
        {


            var test = new List<MotivationLetterReadModel>();


            foreach (var item in model)
            {

                var entity = new LinkUserMotivationLetter
                {
                    UserId = (int)item.UserId,
                    FundingCallId = (int)item.FundingCallId,
                    DocumentId = item.DocumentId,
                    DocumentExtention = item.DocumentExtention,
                    DocumentFile = item.DocumentFile,
                    Filename = item.Filename,
                    UploadType = item.UploadType,
                    DateAdded = DateTime.UtcNow
                };

                // _context.Set<LinkUserMotivationLetter>().Add(entity);

                await _context.LinkUserMotivationLetter.AddAsync(entity);

            }

            await _context.SaveChangesAsync();

            return null;
        }



        public async Task<bool> SubmitApplication(ApplicationSubmissionModel model)
        {
            var application = _context.Applications.FirstOrDefault(a => a.Id == model.ApplicationId);

            if (application != null)
            {
                // Track if this is a resubmission (application was previously returned for info)
                bool isResubmission = application.ApplicationStatus?.Status?.ToLower() == "returned for info" ||
                                       application.RFIByFundAdmin == "Yes";
                // Update the new properties
                application.FlightsChooseCheapest = model.FlightsChooseCheapest; // or false
                application.FlightsCheapestExplanation = model.FlightsCheapestExplanation;
                application.AccomChooseCheapest = model.AccomChooseCheapest; // or true
                application.AccomCheapestExplanation = model.AccomCheapestExplanation;
                application.ApplicationStatusId = _context.ApplicationStatus.FirstOrDefault(s => s.StatusName.ToLower().Trim() == ApplicationStatusEnum.PendingApprovalByHod.GetDescription().ToLower().Trim())?.ApplicationStatusId ?? application.ApplicationStatusId;
                application.FinancialMotivation = model.FinancialMotivation;
                application.ApplicantProgress = model.ApplicantProgress;
                application.OutputMeasure = model.OutputMeasure;
                application.OtherFunding = model.OtherFunding.ToString();
                application.FacultyContibution = model.FacultyContibution.ToString();
                application.DepartmentContribution = model.DepartmentContribution.ToString();
                application.ResearchFundsContribution = model.ResearchFundsContribution.ToString();
                application.DHETFundsRequested = model.DHETFundsRequested.ToString();
                application.OtherFundingSource = model.OtherFundingSource;
                application.RFIByFundAdmin = "No"; //  Reset RFI flag
                application.LastModifiedDate = DateTime.Now; //  Update last modified date
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                    throw;
                }
                // ADD THIS: Send resubmission notification to HOD if this was a resubmission
                if (isResubmission)
                {
                    try
                    {
                        var emailModel = new ApplicationDetailsViewModel
                        {
                            Id = application.Id,
                            ReferenceNumber = application.ReferenceNumber,
                            UserId = application.Applicant?.UserId ?? 0,
                            FundingCallDetailsId = application.FundingCalls?.Id ?? 0
                        };

                        await _emailSentRepository.SendResubmittedApplicationToHODEmail(emailModel);
                    }
                    catch (Exception emailEx)
                    {
                        SaveErrorLog("SubmitApplication", "Error sending resubmitted application email: " + emailEx.Message, application.ReferenceNumber);
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            return true;
        }


        public async Task<MotivationLetterResponse> GetLinkUserMotivationLetter(int userId)
        {


            var now = DateTime.UtcNow;
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfNextYear = startOfYear.AddYears(1);

            var entity = await _context.Set<LinkUserMotivationLetter>()
              .AsNoTracking()
              .Where(x => x.UserId == userId
                     && x.DateAdded >= startOfYear
                     && x.DateAdded < startOfNextYear)

              .FirstOrDefaultAsync(


              );


            if (entity is null)
                return null;

            return new MotivationLetterResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                FundingCallId = entity.FundingCallId,
                DocumentId = entity.DocumentId,
                DateAdded = now,
                DocumentExtention = entity.DocumentExtention,
                Filename = entity.Filename,
                UploadType = entity.UploadType
            };

        }

        public async Task<MotivationLetterReadModel> ViewMotivationLetterById(int documentId)
        {


            var now = DateTime.UtcNow;
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfNextYear = startOfYear.AddYears(1);

            var entity = await _context.Set<LinkUserMotivationLetter>()
              .AsNoTracking()
              .Where(x => x.Id == documentId)

              .FirstOrDefaultAsync(


              );


            if (entity is null)
                return null;

            return new MotivationLetterReadModel
            {
                Id = entity.Id,
                DocumentFile = entity.DocumentFile,
                DocumentId = entity.DocumentId,
                DateAdded = now,
                DocumentExtention = entity.DocumentExtention,
                Filename = entity.Filename,
                UploadType = entity.UploadType
            };

        }

        public async Task<bool> DeleteMotivationLetterById(int documentId)
        {
            try
            {
                var entity = await _context.Set<LinkUserMotivationLetter>()
                    .FirstOrDefaultAsync(x => x.Id == documentId);

                if (entity == null)
                    return false;

                _context.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<Applications> UpdateApplicantDetails(ApplicationDetailsViewModel model)
        {
            if (model == null || model.Id <= 0)
                return null;

            var entity = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.FundingCalls)
                .Include(a => a.ApplicationStatus)
                .FirstOrDefaultAsync(a => a.Id == model.Id);

            if (entity == null)
                return null;

            entity.LastSavedStep = model.LastSavedStep;
            entity.FundingStartDate = model.FundingStartDate;
            entity.FundingEndDate = model.FundingEndDate;
            entity.ApplicantCategory = model.ApplicantCategory;
            entity.AppointmentCategory = model.AppointmentCategory;
            entity.CostCentreName = model.CostCentreName;
            entity.CostCentreNumber = model.CostCentreNumber;
            entity.PreviousFundingYear = model.PreviousFundingYear;
            entity.PreviousFundingAmount = model.PreviousFundingAmount;
            entity.PreviousFundingOutcome = model.PreviousFundingOutcome;
            entity.LastModifiedDate = DateTime.Now;

            if (model.FundingCallDetailsId > 0)
            {
                var fundingCall = await _context.FundingCalls
                    .FirstOrDefaultAsync(f => f.Id == model.FundingCallDetailsId);

                if (fundingCall != null)
                    entity.FundingCalls = fundingCall;
            }

            // Keep it incomplete while still editing the form
            var incompleteStatus = await _context.ApplicationStatus
                .FirstOrDefaultAsync(s => s.Status.ToLower() == "incomplete");

            if (incompleteStatus != null)
                entity.ApplicationStatus = incompleteStatus;

            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<Applications> SaveImprovementStaffQualifications(ApplicationDetailsViewModel model)
        {
            try
            {
                var application = await _context.Applications
                    .FirstOrDefaultAsync(a => a.Id == model.Id);

                if (application == null)
                    return null;

                application.StudyingTowards = model.StudyingTowards;
                application.FirstYearRegistration = model.FirstYearRegistration;
                application.PlannedGraduationYear = model.PlannedGraduationYear;
                application.Describe = model.Describe;
                application.AppointmentDescribe = model.AppointmentDescribe;
                application.FieldOfStudy = model.FieldOfStudy;
                application.TitleOfThesis = model.TitleOfThesis;
                application.LastSavedStep = model.LastSavedStep;

                application.SupportRequired = model.SupportRequired != null
                    ? string.Join(",", model.SupportRequired)
                    : null;

                application.AppointmentOption = model.AppointmentOption != null
                    ? string.Join(",", model.AppointmentOption)
                    : null;

                await _context.SaveChangesAsync();

                return application;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving staff improvement details.", ex);
            }
        }




        public async Task<List<Applications>> GetAllIncompleteApplicationsByUserId(int userId)
        {
            try
            {
                if (userId == 0) return null;
                var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return null;
                var application = await _context.Applications.Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).Where(f => f.ApplicantUserStoreUserId == user.UserId && f.ApplicationStatus.Status.ToLower() == ApplicationStatusEnum.Incomplete.GetDescription().ToLower()).ToListAsync();

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }

        public async Task<List<Applications>> SearchCompleteApplicationsByUserId(int userId)
        {
            try
            {
                if (userId == 0) return null;
                var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return null;
                var application = await _context.Applications.Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).Where(f => f.ApplicantUserStoreUserId == user.UserId && f.ApplicationStatus.Status.ToLower() != ApplicationStatusEnum.Incomplete.GetDescription().ToLower()).ToListAsync();

                return application;

            }
            catch (Exception msg)
            {
                throw new NotImplementedException(msg.ToString());
            }
        }
    }

}



