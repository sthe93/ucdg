using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Common;

namespace UDCG.Application.Feature.Application.Resources
{
    public class GetCostCentreDataQuery
    {
        public string CostCentreNumber { get; set; }
        public ApiIntegrationCircleModel ApiIntegrationCircleModel { get; set; }
        public ApiIntegrationCircleAPICostCentreModel ApiIntegrationCircleAPICostCentreModel { get; set; }
    }
}
