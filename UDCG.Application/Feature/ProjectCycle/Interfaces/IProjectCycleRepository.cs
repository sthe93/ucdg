using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.ProjectCycle.Interfaces
{
    public interface IProjectCycleRepository
    {
        Task<List<ProjectCycles>> GetAllProjectCycles();

        Task<ProjectCycles> GetActiveProjectCycle();
        Task<ProjectCycles>  GetProjectCycleById(int Id);

        Task<ProjectCycles> Add(ProjectCycles model);

        Task<ProjectCycles> Update(ProjectCycles model);
        Task<ProjectCycles> GetProjectCycleByPeriod(string period);


    }
}
