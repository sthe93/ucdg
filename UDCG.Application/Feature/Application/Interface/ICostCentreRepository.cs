
using System.Threading.Tasks;
using UDCG.Application.Feature.Application.Resources;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface ICostCentreRepository
    {
        //Task<CostCentreViewModel> GetCostCentreByCode(string centreCode);  
        Task<CostCentreNumberReadModel> GetCostCentreByCode(GetCostCentreDataQuery model);
    }
}
