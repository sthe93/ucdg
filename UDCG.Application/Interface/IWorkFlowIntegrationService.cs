using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Interface
{
    public interface IWorkFlowIntegrationService
    {
        Task<ApplicationStatus> GetApplicationStatusByRoleName(string RoleName, string status, string currentUsername);

        Task<ApplicationStatus> GetApplicationStatusByRoleNameV2(string RoleName, string status, UserStoreUser user);

        string UpdateStatus(ApplicationStatus status, Guid referenceId, string username, string roleName);

        bool UpdateStatusV2(ApplicationStatus status, Guid referenceId, UserStoreUser user, string role);

        //string ActivateEmailRemindersBulkProcess(List<Applications> application);
        public bool CheckIfNextApproverIsMEC(string empNo);
    }
}
