using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.Roles.Resources;
using UDCG.Application.Feature.Users.Resources;

namespace UDCG.Application.Feature.Roles.Interfaces
{
    public interface IUserRepository
    {
        public string Username { get; set; }
        Task<UCDG.Domain.Entities.UserStoreUser> Add(UCDG.Domain.Entities.UserStoreUser model);
        Task<bool> Exists();
        Task<UCDG.Domain.Entities.UserStoreUser> GetMyProfileDetails();
        Task<ReadUserResource> UpdateMyProfile(ReadUserResource user);
        Task<UCDG.Domain.Entities.UserStoreUser> SyncProfile(string userName);
        Task<string> ConfirmMyProfile(string username);
        Task<UserRole> AssignUserRole(int userId, string roleName, int createdById, DateTime expiryDate);
        Task<string> AssignTemporaryApprover(int userId, string roleName, string[] RefNumbers, string TempRole);
        Task<string> AssignUserRoles(int userId, List<ReadRoleResource> newRoles, int createdById, DateTime expiryDate);
        Task<UserRole> AssignRole(int userId, string roleName, int createdById, DateTime expiryDate);
        Task<List<UCDG.Domain.Entities.UserStoreUser>> AllUsers(UserParams userParams);
        Task<string> DeActivateUser();
        Task<string> ActivateUser();
        Task<bool> IsUserActive();
        Task <TemporaryApproverViewModel> GetUserTempApprovals(string userName);
       void  SyncMecList();
    }
}
