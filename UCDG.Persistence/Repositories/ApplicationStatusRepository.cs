using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application.Interface;

namespace UCDG.Persistence.Repositories
{
    public class ApplicationStatusRepository : IApplicationStatusRepository 
    {
        private readonly UCDGDbContext _context;

        public ApplicationStatusRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicationStatus>> GetAllApplicationStatus()
        {
            try
            {
                // var applicationStatuses = new List<ApplicationStatus>();

                //applicationStatuses = await _context.ApplicationStatus.ToListAsync();

                var applicationStatuses = await _context.ApplicationStatus
                    .Where(a => a.Status != null && a.StatusName != null)
                    .ToListAsync();


                // Manually add the required statuses
                applicationStatuses.AddRange(new List<ApplicationStatus>
                {
                    new() { ApplicationStatusId = 1002, Status = "Pending Approval By HOD", StatusName = "Pending Approval by HOD" },
                    new() { ApplicationStatusId = 1005, Status = "Pending Approval By ED/VD", StatusName = "Pending Approval by ED or VD" },
                    new() { ApplicationStatusId = 1006, Status = "Pending Approval By FA", StatusName = "Pending Approval by FA" },
                    new() { ApplicationStatusId = 1007, Status = "Pending Approval By SIA", StatusName = "Pending Approval by SIA" },
                    new()  { ApplicationStatusId = 1008, Status = "Pending Approval By FBP", StatusName = "Pending Approval by FBP" }
                });

                return applicationStatuses.ToList();
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<List<ProjectCycles>> GetAllProjectCycles()
        {
            try
            {
                List<ProjectCycles> projectCycles = new List<ProjectCycles>();
                projectCycles = await _context.ProjectCycles.ToListAsync();

                return projectCycles.ToList();
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<ApplicationStatus> ApplicationStatusByName(string statusName)
        {
           return await _context.ApplicationStatus.FirstOrDefaultAsync(o => o.StatusName.ToLower() == statusName.ToLower());
        }
    }
}
