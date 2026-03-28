using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.ApplicationProject.Interface;

namespace UCDG.Persistence.Repositories
{
    public class ApplicationsProjectsRepository : IApplicationProjectRepository
    {
        private readonly UCDGDbContext _context;

        public ApplicationsProjectsRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicationsProjects>> GetApplicationsProjectsById(int applicationId)
        {
            try
            {
                if (applicationId != 0)
                {
                    List<ApplicationsProjects> applicationsProjects = await _context.ApplicationsProjects.Where(u => u.ApplicationsId == applicationId).ToListAsync();

                    return applicationsProjects;

                }

                return null;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }
    }
}
