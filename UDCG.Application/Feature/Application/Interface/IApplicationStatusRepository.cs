using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface IApplicationStatusRepository
    {
        Task<List<ApplicationStatus>> GetAllApplicationStatus();
        Task<ApplicationStatus> ApplicationStatusByName(string statusName);
    }
}
