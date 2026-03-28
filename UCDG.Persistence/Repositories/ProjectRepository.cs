using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Project.Interface;
using System.Linq;




namespace UCDG.Persistence.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly UCDGDbContext _context;

        public ProjectRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<Projects> Add(Projects model)
        {
            try
            {
              
                if (!string.IsNullOrEmpty(model.ProjectCycleId))
                {
                    ProjectCycles cycles = _context.ProjectCycles.Where(c => c.Id == Convert.ToInt32(model.ProjectCycleId)).FirstOrDefault();

                    if (cycles != null)
                    {
                        model.ProjectCycles = cycles;

                    }

                }

                var results = await _context.Projects.AddAsync(model);
                _context.SaveChanges();             

                return model;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<List<Projects>> GetAllProjectAndCycles()
        {
            try
            {
                List<Projects> project = new List<Projects>();
                project = await _context.Projects.Include(c => c.ProjectCycles).ToListAsync();

                return project.ToList();
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<List<Projects>> GetActiveCycleProjects()
        {
            try
            {
                List<Projects> projects = new List<Projects>();
                projects= await _context.Projects.Include(c => c.ProjectCycles).Where(c => c.ProjectCycles.IsActive == true && c.IsActive == true).ToListAsync();

                return projects.ToList();
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<List<Projects>> SearchProjects(string Name, int ProjectCycleId)
        {
            try
            {
                List<Projects> projects = new List<Projects>();
                projects = await _context.Projects.Include(c => c.ProjectCycles).ToListAsync();

                if(!string.IsNullOrEmpty(Name))
                {
                    projects = await _context.Projects.Include(c => c.ProjectCycles).Where(c => c.ProjectName.Contains(Name)).ToListAsync();
                }

                if (ProjectCycleId != 0)
                {
                    projects = await _context.Projects.Include(c => c.ProjectCycles).Where(c => c.ProjectCycles.Id == ProjectCycleId).ToListAsync();
                }

                return projects.ToList();
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<Projects> Update(Projects model)
        {
            try
            {

                if (model.Id != 0)
                {
                    Projects entity = await _context.Projects.FirstOrDefaultAsync(f => f.Id == model.Id);

                    if (!string.IsNullOrEmpty(model.ProjectCycleId))
                    {
                        ProjectCycles cycles = _context.ProjectCycles.Where(c => c.Id == Convert.ToInt32(model.ProjectCycleId)).FirstOrDefault();

                        if (cycles != null)
                        {
                            model.ProjectCycles = cycles;

                        }
                    }

                    if (entity != null)
                    {
                        entity.ProjectName   = model.ProjectName;
                        entity.ProjectCycles = model.ProjectCycles;
                        entity.IsActive      = model.IsActive;

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

        public async Task<Projects> GetProjectById(int Id)
        {
            try
            {
                Projects project = new Projects();

                if (Id != 0)
                {
                    project    = await _context.Projects.Include(c => c.ProjectCycles).FirstOrDefaultAsync(f => f.Id == Id);
                    project.ProjectCycleId = project.ProjectCycles.Id.ToString();
                }

                return project;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }
    }
}
