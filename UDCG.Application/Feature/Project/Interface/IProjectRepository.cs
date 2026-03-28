using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.Project.Interface
{
    public interface IProjectRepository
    {
        Task<List<Projects>> SearchProjects(string Name, int ProjectCycleId);

        Task<List<Projects>> GetActiveCycleProjects();

        Task<List<Projects>> GetAllProjectAndCycles();

        Task<Projects> Add(Projects model);

        Task<Projects> Update(Projects model);

        Task<Projects> GetProjectById(int Id);
    }
}
