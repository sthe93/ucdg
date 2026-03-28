using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.ApplicationProject.Interface
{
    public interface IApplicationProjectRepository
    {
        Task<List<ApplicationsProjects>> GetApplicationsProjectsById(int applicationId);
    }
}
