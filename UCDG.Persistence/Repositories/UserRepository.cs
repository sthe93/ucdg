using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UCDG.Persistence.Enums;
using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.Roles.Interfaces;
using UDCG.Application.Feature.Roles.Resources;
using UDCG.Application.Feature.Users.Resources;

namespace UCDG.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {

        public string Username { get; set; }
        private readonly UCDGDbContext _context;
        private readonly UserStoreDbContext _userStoreDbContext;
        private readonly IStaffAd _staffAd;
        private readonly IMapper _mapper;

        public UserRepository(UCDGDbContext context, IStaffAd staffAd, IMapper mapper, UserStoreDbContext userStoreDbContext)
        {
            _context = context;
            _staffAd = staffAd;
            _mapper = mapper;
            _userStoreDbContext = userStoreDbContext;
        }

        public async Task<UserStoreUser> Add(UserStoreUser model)
        {

            try
            {
                model.IsActive = true;
                var results = await _userStoreDbContext.Users.AddAsync(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
            return model;

        }

        private IQueryable<UserStoreUser> GetAllUsers()
        {
            return _userStoreDbContext.Users.Include(c => c.UserRoles).ThenInclude(c => c.Role).Include(o => o.Qualifications).AsQueryable();
        }
        public async Task<List<UserStoreUser>> GetUserPageList(IQueryable<UserStoreUser> results, UserParams userParams)
        {
            var usersList = new List<UserStoreUser>();
            usersList = results.ToList();

            if (!string.IsNullOrEmpty(userParams.Username))
                usersList = await results.Where(o => o.Username.ToLower().Contains(userParams.Username.ToLower())
                                                || o.Title.ToLower().Contains(userParams.Username.ToLower())
                                                || o.Name.ToLower().Contains(userParams.Username.ToLower())
                                                || o.Surname.ToLower().Contains(userParams.Username.ToLower())
                                                || o.CellPhone.ToLower().Contains(userParams.Username.ToLower())
                                                || o.Campus.ToLower().Contains(userParams.Username.ToLower())
                                                || o.Email.ToLower().Contains(userParams.Username.ToLower())
                                                || o.Gender.ToLower().Equals(userParams.Username.ToLower())
                                                || o.Department.ToLower().Contains(userParams.Username.ToLower())
                                                || o.FacultyDivision.ToLower().Contains(userParams.Username.ToLower())).ToListAsync();

            if (!string.IsNullOrEmpty(userParams.Username) && usersList.Count() == 0)
                usersList = await _userStoreDbContext.Users.Include(c => c.UserRoles).ThenInclude(c => c.Role).Where(c => c.UserRoles.Any(c => c.Role.Name.ToLower().Contains(userParams.Username.ToLower()) && c.IsActive == true)).ToListAsync();

            return usersList;
        }
        public async Task<List<UserStoreUser>> AllUsers(UserParams userParams)
        {
            var results = GetAllUsers();
            var users = await GetUserPageList(results, userParams);

            return users;
        }
        public async Task<bool> Exists()
        {
            try
            {
                var results = await _userStoreDbContext.Users.FirstOrDefaultAsync(o => o.Username.ToLower() == Username.ToLower()) != null;
                return results;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<UserStoreUser> GetMyProfileDetails()
        {
            var results = await _userStoreDbContext.Users.Include(o => o.Qualifications).Include(i => i.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(o => o.Username.ToLower() == Username.ToLower());
            if (results != null)
            {
                results.UserRoles = results.UserRoles.Where(o => o.IsActive == true).ToList();
                //results.IsFundAdministrator = results.UserRoles.Any(o => o.Role.Name == AssignableUserRoles.FundAdministrator.GetDescription() && o.IsActive == true);
            }
            return results;

            // var results = await _context.Users.Include(o => o.Qualifications).Include(i => i.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(o => o.Username.ToLower() == Username.ToLower());

            //var results = await _context.Users
            //    .Include(u => u.Qualifications)
            //    .Include(u => u.UserRoles)
            //        .ThenInclude(ur => ur.Role)
            //    .Where(u => u.Username.ToLower() == Username.ToLower())
            //    .Select(u => new
            //    {
            //        User = u,
            //        RecentApplication = u.Applications
            //            .Where(a => a.SubmittedDate != null)
            //            .OrderByDescending(a => a.SubmittedDate)
            //            .Select(a => new
            //            {
            //                a.Id,
            //                a.SubmittedDate,
            //                a.FundingStartDate,
            //                a.FundingEndDate,
            //                a.ApplicationStartDate,
            //                a.ApplicationEndDate,
            //                ApplicationStatus = new
            //                {
            //                    a.ApplicationStatus.ApplicationStatusId,
            //                    a.ApplicationStatus.Status,
            //                    a.ApplicationStatus.StatusName
            //                },
            //                ProgressReport = a.ProgressReport != null ? new
            //                {
            //                    a.ProgressReport.Id,
            //                    a.ProgressReport.IsComplete,
            //                    a.ProgressReport.CreatedDate,
            //                    a.ProgressReport.IsQualificationInPrgress,
            //                    ProgressReportStatus = new
            //                    {
            //                        a.ProgressReport.ProgressReportStatus.ProgressReportStatusId,
            //                        a.ProgressReport.ProgressReportStatus.Status
            //                    }
            //                } : null
            //            })
            //            .FirstOrDefault()
            //    })
            //    .FirstOrDefaultAsync();

            //if (results != null)
            //{
            //    results.User.UserRoles = results.User.UserRoles.Where(o => o.IsActive == true).ToList();
            //    results.User.IsFundAdministrator = results.User.UserRoles.Any(o => o.Role.Name == AssignableUserRoles.FundAdministrator.GetDescription() && o.IsActive == true);
            //    return results.User;
            //}
            //return null;



        }

        public async Task<ReadUserResource> UpdateMyProfile(ReadUserResource user)
        {
            var entity = await _userStoreDbContext.Users.FirstOrDefaultAsync(f => f.Username.ToLower() == user.Username.ToLower());
            if (entity != null)
            {

                entity.ModifiedDate = DateTime.Now;
                entity.AlternativeEmailAddress = user.AlternativeEmailAddress;
                //entity.OtherProgramme = user.OtherProgramme;
                //entity.OtherProgrammeName = user.OtherProgrammeName;
                //entity.OtherFundSource = user.OtherFundSource;
                //entity.OtherFundSourceName = user.OtherFundSourceName;
                entity.IsProfileCompleted = true;
                entity.CostCentreNumber = user.CostCentreNumber;

                user.UserId = entity.UserId;

                await _context.SaveChangesAsync();
            }
            return user;
        }

        public void SyncMecList()
        {
            var resultInfo = new List<MecMembers>();

            var mecListFromService = _staffAd.GetMecList();
            var existingMecMembers = _context.MecMembers.ToList();

            foreach (var mecMember in mecListFromService)
            {
                var existingMember = existingMecMembers.FirstOrDefault(m => m.EmployeeNo == mecMember.EmployeE_NUMBER);

                if (existingMember != null)
                {
                    existingMember.Title = mecMember.Title;
                    existingMember.Initials = mecMember.Initials;
                    existingMember.LastName = mecMember.LasT_NAME;
                    existingMember.LastModified = DateTime.UtcNow;
                }
                else
                {
                    _context.MecMembers.Add(new MecMembers()
                    {
                        EmployeeNo = mecMember.EmployeE_NUMBER,
                        Title = mecMember.Title,
                        Initials = mecMember.Initials,
                        LastName = mecMember.LasT_NAME,
                        CreatedDate = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow
                    });
                }
            }

            _context.SaveChangesAsync();
        }

        public async Task<UserStoreUser> SyncProfile(string userName)
        {
            var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(f => f.Username.ToLower() == userName.ToLower());

            if (user != null)
            {

                var staffNumber = await _staffAd.GetStaffNumber(user.Username);

                ReadUserViewModelResource userDetails = _staffAd.GetUserOracleProfile(staffNumber, user.Username);

                if (userDetails != null)
                {



                    user.Username = userDetails.Username;
                    user.Surname = userDetails.Surname;
                    user.Name = userDetails.FirstName;
                    user.Nationality = userDetails.Nationality;
                    user.IdNumber = userDetails.IdPassportNumber;
                    user.DateOfBirth = userDetails.DateOfBirth;
                    user.Email = userDetails.EmailAddress;
                    user.CellPhone = userDetails.CellPhoneNumber;
                    // user.li = userDetails.TelephoneNumber;
                    user.Campus = userDetails.Campus;
                    user.Title = userDetails.Title;
                    user.HRPostNumber = userDetails.StaffNumber;
                    user.Position = userDetails.Position;
                    user.IsAcademic = userDetails.IsAcademic;
                    user.FacultyDivision = userDetails.Faculty;
                    user.Department = userDetails.Department;
                    user.Race = userDetails.Race;
                    user.Gender = userDetails.Gender;
                    user.Disability = userDetails.Disability;
                    user.LineManagerName = userDetails.HOD;
                    user.LineManagerStaffNumber = userDetails.HODStaffNUmber;
                    user.ViceDean = userDetails.ViceDean;
                    user.ViceDeanStaffNumber = userDetails.ViceDeanStaffNUmber;
                    user.HodPersonType = userDetails.HodPersonType;
                    user.ViceDeanPersonType = userDetails.ViceDeanPersonType;

                    await _context.SaveChangesAsync();
                }
            }

            //ReadUserViewModelResource readUserViewModel = _staffAd.GetUserOracleProfile(user.StaffNumber, user.Username);

            //if (readUserViewModel != null)
            //{
            //    if (readUserViewModel.Qualifications.Count() > 0)
            //    {
            //        List<Qualification> userQual = await _context.Qualifications.Where(c => c.UserId == user.UserId).ToListAsync();

            //        foreach (var items in userQual)
            //        {
            //            _context.Qualifications.Remove(items);
            //            _context.SaveChanges();
            //        }

            //        foreach (var qual in readUserViewModel.Qualifications)
            //        {
            //            Qualification qualification = new Qualification
            //            {
            //                UserId            = user.UserId,
            //                Name              = qual.Name,
            //                InstitutionName   = qual.InstitutionName,
            //                QualificationType = qual.QualificationType,
            //                CreatedDate       = DateTime.Now,
            //                ModifiedDate      = DateTime.Now,

            //            };

            //            var updatedQual = await _context.Qualifications.AddAsync(qualification);
            //            _context.SaveChanges();
            //        }
            //    }
            //}

            return user;
        }

        public async Task<string> ConfirmMyProfile(string userName)
        {
            string results = "0";

            var user = await _userStoreDbContext.Users
                              .FirstOrDefaultAsync(f => f.Username.ToLower() == userName.ToLower());
            if (user != null)
            {

                user.IsProfileCompleted = true;
                user.ModifiedDate = DateTime.Now;
                results = user.UserId.ToString();

                await _context.SaveChangesAsync();

            }
            if (int.Parse(results) > 0)
            {
                results = "Success";
            }
            return results;
        }
        public async Task<UserStoreUser> CreateHOD(string username1)
        {

            var user = await _userStoreDbContext.Users.Include(o => o.Qualifications).Include(o => o.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(o => o.Username.ToLower() == username1.ToLower());


            var username = await _staffAd.GetStaffUsername(user.LineManagerStaffNumber);
            //Create HOD
            //check if user exists
            var sdd = new UserStoreUser();
            try
            {
                sdd = await CheckUser(sdd);
            }
            catch (Exception ex)
            {


            }
            if (sdd != null)
            {
                var userex = await _userStoreDbContext.Users.Include(o => o.Qualifications).Include(o => o.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(o => o.Username.ToLower() == username.ToLower());
                var hodRole = await AssignUserRole(userex.UserId, "HOD", userex.UserId, DateTime.Now.AddYears(1));//HOD is also be applicant
            }

            var userdetails = _staffAd.GetUserOracleProfile(user.LineManagerStaffNumber, username);

            var newUser = _mapper.Map<UserStoreUser>(userdetails);
            var createdUser = await Add(newUser);
            var rr = await AssignUserRole(createdUser.UserId, "HOD", createdUser.UserId, DateTime.Now.AddYears(1));//HOD is also be applicant
            return createdUser;
        }

        private async Task<UserStoreUser> CheckUser(UserStoreUser sdd)
        {
            sdd = await _userStoreDbContext.Users.FirstOrDefaultAsync(o => o.Username.ToLower() == Username.ToLower());
            return sdd;
        }

        public async Task<UserRole> AssignUserRole(int userId, string roleName, int createdById, DateTime expiryDate)
        {

            var role = await _userStoreDbContext.Roles.FirstOrDefaultAsync(o => o.Name.ToLower() == roleName.ToLower());

            var res = await _userStoreDbContext.UserRoles.Where(o => o.UserId == userId).ToListAsync();


            foreach (var item in res)
            {
                var userRoleResult = await _userStoreDbContext.UserRoles.FirstOrDefaultAsync(o => o.UserId == item.UserId && o.Role.RoleId == item.RoleId);
                if (userRoleResult != null)
                {
                    userRoleResult.IsActive = false;
                    userRoleResult.DateModified = DateTime.Now;
                    userRoleResult.ModifiedBy = createdById;
                    _context.SaveChanges();
                }
            }

            //TO DO confirm multi roles

            var userRole = await _userStoreDbContext.UserRoles.FirstOrDefaultAsync(o => o.UserId == userId & o.RoleId == role.RoleId);
            if (userRole == null)
            {
                var newUserRole = new UserRole()
                {
                    RoleId = role.RoleId,
                    UserId = userId,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    CreatedBy = createdById,
                    IsActive = true,
                    ModifiedBy = createdById,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                };
                _userStoreDbContext.UserRoles.Add(newUserRole);
                _context.SaveChanges();
                return newUserRole;
            }
            else
            {
                var userRoles = await _userStoreDbContext.UserRoles.Where(o => o.UserId == userId & o.RoleId == role.RoleId).ToListAsync();
                foreach (var item in userRoles)
                {
                    item.IsActive = false;
                    _context.SaveChanges();
                }

                userRole.DateModified = DateTime.Now;
                userRole.ModifiedBy = createdById;
                userRole.ExpiryDate = expiryDate;
                userRole.IsActive = true;

                _context.SaveChanges();

                return userRole;
            }



        }

        public async Task<string> AssignTemporaryApprover(int userId, string roleName, string[] RefNumber, string TempRole)
        {
            var role = _userStoreDbContext.Roles.FirstOrDefault(o => o.Name.ToLower() == roleName.ToLower());

            //Assign References to Temporary Approver
            if (role.Name.ToLower().Trim() == AssignableUserRoles.TemporaryApprover.GetDescription().ToLower().Trim())
            {

                if (RefNumber != null && RefNumber.Any())
                {
                    List<TemporaryApproverApplications> temporaryApprovers = await _context.TemporaryApproverApplications.Where(s => s.User.UserId == userId).ToListAsync();
                    foreach (var items in temporaryApprovers)
                    {
                        _context.TemporaryApproverApplications.Remove(items);
                        _context.SaveChanges();
                    }

                    foreach (var applicationId in RefNumber)
                    {
                        Applications application = await _context.Applications.Where(s => s.Id == Convert.ToInt32(applicationId)).FirstOrDefaultAsync();

                        UserStoreUser user = await _userStoreDbContext.Users.Where(s => s.UserId == userId).FirstOrDefaultAsync();

                        if (application.Id != 0 && user.UserId != 0)
                        {
                            _context.AddRange(new TemporaryApproverApplications { Applications = application, User = user, ApprovedAs = TempRole });
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    return "Error: Please select reference number for Temporary approver";
                }
            }

            return "";

        }

        public async Task<UserRole> AssignRole(int userId, string roleName, int createdById, DateTime expiryDate)
        {
            var role = _userStoreDbContext.Roles.FirstOrDefault(o => o.Name.ToLower() == roleName.ToLower());

            var userRole = await _userStoreDbContext.UserRoles.FirstOrDefaultAsync(o => o.UserId == userId & o.RoleId == role.RoleId);
            if (userRole == null)
            {
                var newUserRole = new UserRole()
                {
                    RoleId = role.RoleId,
                    UserId = userId,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    CreatedBy = createdById,
                    IsActive = true,
                    ModifiedBy = createdById,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now
                };
                _userStoreDbContext.UserRoles.Add(newUserRole);
                _context.SaveChanges();
                return newUserRole;
            }
            else
            {
                userRole.DateModified = DateTime.Now;
                userRole.ModifiedBy = createdById;
                userRole.ExpiryDate = expiryDate;
                userRole.IsActive = true;

                _context.SaveChanges();

                return userRole;
            }

        }
        private async Task<Role> GetRole(string roleName)
        {
            var results = await _userStoreDbContext.Roles.FirstOrDefaultAsync(o => o.Name.ToLower() == roleName.ToLower());
            return results;
        }

        public async Task<TemporaryApproverViewModel> GetUserTempApprovals(string userName)
        {
            TemporaryApproverViewModel model = new TemporaryApproverViewModel();

            List<TemporaryApproverApplications> approvals = new List<TemporaryApproverApplications>();

            List<string> refIds = new List<string>();
            UserStoreUser userDetails = await _userStoreDbContext.Users.FirstOrDefaultAsync(o => o.Username.Trim().ToLower() == userName.Trim().ToLower());

            if (userDetails != null)
            {
                approvals = await _context.TemporaryApproverApplications.Include(c => c.User).Include(c => c.Applications).Where(c => c.User.UserId == userDetails.UserId).ToListAsync();

                foreach (var item in approvals)
                {
                    var references = new List<string>();
                    TemporaryApproverViewModel viewModel = new TemporaryApproverViewModel();
                    refIds.Add(item.Applications.Id.ToString());

                }

                model.ReferenceNumberIds = refIds.ToArray();
            }

            return model;
        }

        public async Task<string> AssignUserRoles(int userId, List<ReadRoleResource> newRoles, int createdById, DateTime expiryDate)
        {
            var msg = "";
            var res = await _userStoreDbContext.UserRoles.Where(o => o.UserId == userId).ToListAsync();

            try
            {
                foreach (var userRole in res)
                {
                    var userRoleResult = await _userStoreDbContext.UserRoles.FirstOrDefaultAsync(o => o.UserId == userRole.UserId && o.Role.RoleId == userRole.RoleId);
                    if (userRoleResult != null)
                    {
                        userRoleResult.IsActive = false;
                        userRoleResult.DateModified = DateTime.Now;
                        userRoleResult.ExpiryDate = expiryDate;
                        userRoleResult.ModifiedBy = createdById;
                        _context.SaveChanges();
                    }
                }

                foreach (var item in newRoles)
                {
                    var userRol = await _userStoreDbContext.UserRoles.FirstOrDefaultAsync(o => o.UserId == userId && o.Role.RoleId == item.RoleId);
                    if (userRol != null)
                    {
                        userRol.IsActive = true;
                        userRol.DateModified = DateTime.Now;
                        userRol.ModifiedBy = createdById;
                        _context.SaveChanges();
                    }
                    else
                    {
                        var newUserRole = new UserRole();
                        newUserRole.RoleId = item.RoleId;
                        newUserRole.UserId = userId;
                        newUserRole.CreatedBy = newUserRole.ModifiedBy = createdById;
                        newUserRole.DateCreated = newUserRole.DateModified = DateTime.Now;
                        newUserRole.IsActive = true;
                        newUserRole.ExpiryDate = expiryDate;

                        _userStoreDbContext.UserRoles.Add(newUserRole);
                        _context.SaveChanges();
                    }
                }

                msg = "Save Successfully";
            }
            catch (Exception e)
            {
                msg = "Failed to save record, error: " + e.Message;
            }

            return msg;
        }

        public async Task<string> DeActivateUser()
        {
            try
            {
                var results = await _userStoreDbContext.Users.FirstOrDefaultAsync(o => o.Username.ToLower() == Username.ToLower());
                if (results == null) return "Error: Failed to save record";
                results.IsActive = false;
                _context.SaveChanges();
                return "User deactivated"; ;

            }
            catch (Exception)
            {
                return "Error: Failed to save record";
            }
        }
        public async Task<string> ActivateUser()
        {
            try
            {
                var results = await _userStoreDbContext.Users.FirstOrDefaultAsync(o => o.Username.ToLower() == Username.ToLower());
                if (results != null)
                {
                    results.IsActive = true;
                    _context.SaveChanges();
                    return "User activated";
                }
                return "Error: Failed to save record";
            }
            catch (Exception)
            {
                return "Error: Failed to save record";
            }
        }
        public async Task<bool> IsUserActive()
        {
            try
            {
                var results = await _userStoreDbContext.Users.FirstOrDefaultAsync(o => o.Username.ToLower() == Username.ToLower());
                if (results != null)
                    return results.IsActive;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
