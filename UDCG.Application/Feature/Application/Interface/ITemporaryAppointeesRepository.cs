using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface ITemporaryAppointeesRepository
    {
        Task<TemporaryAppointees> Add(TemporaryAppointeeViewModel model);
        Task<TemporaryAppointees> UpdateTemporaryAppointee(int applicationsId);
        Task<List<TemporaryAppointeeViewModel>> GetTemporaryAppointeesByApplicationId(int applicationsId);
        Task<int> DeleteTemporaryAppointee(int TemporaryAppointeeId);
        Task<TemporaryAppointees> SaveStaffImprovementTemporaryAppointee(TemporaryAppointeeViewModel model);
    }
}
