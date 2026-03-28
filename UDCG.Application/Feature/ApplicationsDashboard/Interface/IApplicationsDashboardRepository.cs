using System.Collections.Generic;
using System.Threading.Tasks;
using UDCG.Application.Feature.ApplicationsDashboard.Resources;

namespace UDCG.Application.Feature.ApplicationsDashboard.Interface
{
    public interface IApplicationsDashboardRepository
    {
        Task<List<ApplicationRowDto>> GetInboxAsync(string staffNumber, int? userStoreUserId);
        Task<List<ApplicationRowDto>> GetMyApplicationsAsync(string staffNumber, int? userStoreUserId);
        // Task<List<ApplicationRowDto>> GetProcessedAsync(string staffNumber, string username, ProcessedViewMode mode);
        Task<List<ApplicationRowDto>> GetMyTeamAsync(string staffNumber, bool canViewAllApplications, bool isSiaDirector);
        Task<DashboardCapsDto> GetDashboardCapsAsync(int userStoreUserId, string staffNumber, bool isFundAdmin, bool isSiaDirector);
    }
}
