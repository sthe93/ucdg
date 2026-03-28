using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDCG.Application.Feature.ApplicationsAdmin.Resources;

namespace UDCG.Application.Feature.ApplicationsAdmin.Interface
{
    public interface IApproverRefreshService
    {
        Task<ApproverRefreshResult> RefreshActiveApproverAssignmentsAsync(int actorUserStoreUserId);
        Task<(string? FundAdminStaffNo, string? SiaDirectorStaffNo)> GetCurrentRoleOwnersAsync();
    }
}
