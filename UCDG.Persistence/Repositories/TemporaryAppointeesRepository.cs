using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application;
using UDCG.Application.Feature.Application.Interface;

namespace UCDG.Persistence.Repositories
{
    public class TemporaryAppointeesRepository : ITemporaryAppointeesRepository
    {
        private readonly UCDGDbContext _context;

        public TemporaryAppointeesRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<TemporaryAppointees> Add(TemporaryAppointeeViewModel model)
        {
            try
            {
                // List<TemporaryAppointees> savedAppointment = new List<TemporaryAppointees>();

                Applications applications = await _context.Applications.FirstOrDefaultAsync(u => u.Id == model.ApplicationsId);

                //foreach (var appointee in model)
                //{

                TemporaryAppointees temporaryAppointee = new TemporaryAppointees();

                temporaryAppointee.Name = model.Name;
                temporaryAppointee.Surname = model.Surname;
                temporaryAppointee.ContactNumber = model.ContactNumber;
                temporaryAppointee.IdNumber = model.IdNumber;
                temporaryAppointee.EmailAddress = model.EmailAddress;
                temporaryAppointee.StartDate = model.StartDate;
                temporaryAppointee.EndDate = model.EndDate;
                temporaryAppointee.StaffStatus = model.StaffStatus;
                temporaryAppointee.CanDisplay = model.CanDisplay;
                temporaryAppointee.Applications = applications;

                await _context.TemporaryAppointees.AddAsync(temporaryAppointee);
                _context.SaveChanges();
                // model.FirstOrDefault().Id = temporaryAppointee.Id;

                //}

                //await _context.SaveChangesAsync();

                return temporaryAppointee;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }

        }

        public async Task<List<TemporaryAppointeeViewModel>> GetTemporaryAppointeesByApplicationId(int applicationsId)
        {
            try
            {
                List<TemporaryAppointeeViewModel> temp = new List<TemporaryAppointeeViewModel>();

                List<TemporaryAppointees> tempList = await _context.TemporaryAppointees.Include(c => c.Applications).Where(u => u.Applications.Id == applicationsId && u.CanDisplay == true).ToListAsync();

                foreach (var item in tempList)
                {
                    var model = new TemporaryAppointeeViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Surname = item.Surname,
                        EmailAddress = item.EmailAddress,
                        ContactNumber = item.ContactNumber,
                        ApplicationsId = item.Applications.Id,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        IdNumber = item.IdNumber,
                        StaffStatus = item.StaffStatus,
                    };

                    temp.Add(model);

                }

                return temp;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }

        }

        public async Task<TemporaryAppointees> UpdateTemporaryAppointee(int applicationsId)
        {
            try
            {
                TemporaryAppointees temp = new TemporaryAppointees();
                if (applicationsId != 0)
                {
                    List<TemporaryAppointees> tempList = await _context.TemporaryAppointees.Include(f => f.Applications).Where(f => f.Applications.Id == applicationsId && f.CanDisplay == false).ToListAsync();

                    foreach (var item in tempList)
                    {
                        item.CanDisplay = true;
                        _context.TemporaryAppointees.Update(item);
                        await _context.SaveChangesAsync();
                        temp = item;
                    }

                }

                return temp;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }



        public async Task<int> DeleteTemporaryAppointee(int TemporaryAppointeeId)
        {
            try
            {

                if (TemporaryAppointeeId != 0)
                {
                    var temp = await _context.TemporaryAppointees.FirstOrDefaultAsync(f => f.Id == TemporaryAppointeeId);

                    _context.TemporaryAppointees.Remove(temp);
                    await _context.SaveChangesAsync();
                }

                return TemporaryAppointeeId;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }


        public async Task<TemporaryAppointees> SaveStaffImprovementTemporaryAppointee(TemporaryAppointeeViewModel model)
        {
            try
            {
                if (model == null || model.ApplicationsId <= 0)
                {
                    return null;
                }

                var application = await _context.Applications.FirstOrDefaultAsync(a => a.Id == model.ApplicationsId);

                if (application == null)
                {
                    return null;
                }

                TemporaryAppointees entity;

                if (model.Id > 0)
                {
                    entity = await _context.TemporaryAppointees.Include(x => x.Applications).FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (entity == null)
                    {
                        return null;
                    }
                }
                else
                {
                    entity = new TemporaryAppointees();
                    _context.TemporaryAppointees.Add(entity);
                }

                entity.Applications = application;
                entity.Name = model.Name?.Trim();
                entity.Surname = model.Surname?.Trim();
                entity.EmailAddress = model.EmailAddress?.Trim();
                entity.ContactNumber = model.ContactNumber?.Trim();
                entity.StartDate = model.StartDate;
                entity.EndDate = model.EndDate;
                entity.IdNumber = model.IdNumber?.Trim();
                entity.StaffStatus = model.StaffStatus?.Trim();
                entity.CanDisplay = true;

                await _context.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.ToString());
            }
        }
    }
}
