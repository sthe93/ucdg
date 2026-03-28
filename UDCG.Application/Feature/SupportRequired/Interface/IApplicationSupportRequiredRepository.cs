using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.SupportRequired.Interface
{
    public interface IApplicationSupportRequiredRepository
    {
        Task<List<ApplicationSupportRequired>> GetApplicationsSupportRequiredById(int applicationId);
    }
}
