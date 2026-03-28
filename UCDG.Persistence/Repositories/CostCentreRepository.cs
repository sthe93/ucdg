using System; 
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application.Interface;
using UDCG.Application.Feature.Application.Resources;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UDCG.Application.Interface;
using UDCG.Application.Common.Request.Models;

namespace UCDG.Persistence.Repositories
{
    public class CostCentreRepository : ICostCentreRepository
    {
        private readonly UCDGDbContext _context;
        private readonly IRequest _request;

        //public CostCentreRepository(UCDGDbContext context)
        //{
        //    _context = context;
        //}

        public CostCentreRepository(IRequest request)
        {
            _request = request;
        }

        public  Task<CostCentreNumberReadModel> GetCostCentreByCode(GetCostCentreDataQuery request)
        {
            try
            {
                //CostCentreViewModel costCentreViewModel = new CostCentreViewModel();

                //CostCentres costCentres = await _context.CostCentres.FirstOrDefaultAsync(c => c.CostCentreCode.Trim() == centreCode.Trim());

                //if (costCentres != null)
                //{
                //    costCentreViewModel.CostCentreId   = costCentres.CostCentreId;
                //    costCentreViewModel.CostCentreCode = costCentres.CostCentreCode;
                //    costCentreViewModel.Name           = costCentres.Name;

                //}

                //return costCentreViewModel;

                _request.BaseUrl = request.ApiIntegrationCircleAPICostCentreModel.BaseUrl;
                _request.AuthUrl = request.ApiIntegrationCircleAPICostCentreModel.AuthUrl;
                _request.Username = request.ApiIntegrationCircleAPICostCentreModel.Username;
                _request.Password = request.ApiIntegrationCircleAPICostCentreModel.Password;
                _request.IsFormUrlEncoded = false;

                var results =  _request.ExecuteAsJson("CostCentres/costCentreNumber/" + request.CostCentreNumber, HttpVerb.Get, null);
              
                var costCentreInfo = JsonConvert.DeserializeObject<CostCentreNumberReadModel>(results);


                return Task.FromResult(costCentreInfo);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
