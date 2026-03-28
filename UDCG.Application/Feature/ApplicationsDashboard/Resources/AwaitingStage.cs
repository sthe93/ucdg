using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDCG.Application.Feature.ApplicationsDashboard.Resources
{
    public enum AwaitingStage
    {
        Unknown = 0,
        ApplicantAwardDecisionOrInfo = 1,
        FirstLineManager = 2,
        SecondLineManager = 3,
        FundAdmin = 4,
        SiaDirector = 5
    }
}
