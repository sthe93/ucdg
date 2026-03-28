
//using System.ComponentModel;
//using System.Data.Entity;
//using System.Data.SqlTypes;

//using System.Security.Policy;

//using System.Threading;

//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UDCG.Application.Feature.ProjectCycle.Interfaces;
using UCDG.Domain.Entities;
using System.Text.RegularExpressions;

namespace UCDG.Persistence.Repositories
{
    public class ProjectCycleRepository : IProjectCycleRepository
    {
        private readonly UCDGDbContext _context;

        public ProjectCycleRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectCycles> GetActiveProjectCycle()
        {
            try
            {
                ProjectCycles projectCycle = new ProjectCycles();
                projectCycle = await _context.ProjectCycles.FirstOrDefaultAsync(c => c.IsActive);

                return projectCycle;
            }
            catch(Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<List<ProjectCycles>> GetAllProjectCycles()
        {
            try
            {   
                
               await DeactivateOldProjectCyclePeriods();

                List<ProjectCycles> projectCycles = new List<ProjectCycles>();

                projectCycles = await _context.ProjectCycles.ToListAsync();
                
                return projectCycles.ToList();
            }
            catch(Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<ProjectCycles> GetProjectCycleById(int Id)
        {
            try
            {
                ProjectCycles projectCycle = new ProjectCycles();

                if (Id != 0)
                {
                    projectCycle = await _context.ProjectCycles.FirstOrDefaultAsync(f => f.Id == Id);
                }

                return projectCycle;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<ProjectCycles> Update(ProjectCycles model)
        {
            try
            {

                if (model.Id != 0)
                {
                    ProjectCycles entity = await _context.ProjectCycles.FirstOrDefaultAsync(f => f.Id == model.Id);


                    if (entity != null)
                    {
                        entity.Period = model.Period;
                        entity.IsActive = model.IsActive;

                        await _context.SaveChangesAsync();
                    }
                }

                return model;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<ProjectCycles> Add(ProjectCycles model)
        {
            try
            {

                var results = await _context.ProjectCycles.AddAsync(model);
                _context.SaveChanges();

                return model;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<ProjectCycles> GetProjectCycleByPeriod(string period)
        {
            try
            {
                period = Regex.Replace(period, @"\s", "");
                ProjectCycles projectCycle = new ProjectCycles();
                projectCycle = await _context.ProjectCycles.FirstOrDefaultAsync(c => c.Period.Trim().ToLower() == period.Trim().ToLower());

                return projectCycle;
            }
            catch(Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }
        }


        private async Task DeactivateOldProjectCyclePeriods()
        {
            int currentYear = DateTime.Now.Year;

            var allProjectCycles = await _context.ProjectCycles.Where(c => c.IsActive).ToArrayAsync();

            var periodsList = allProjectCycles
                .Where(p => int.Parse(p.Period.Substring(5, 4)) < currentYear)
                .ToList();

            foreach (var item in periodsList)
            {
                item.IsActive = false;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw new NotImplementedException(ex.Message.ToString());
            }
        }


    }
}
