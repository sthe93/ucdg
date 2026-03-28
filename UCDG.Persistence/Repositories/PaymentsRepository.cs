using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UCDG.Persistence.Enums;
using UDCG.Application.Feature.Application;
using UDCG.Application.Feature.Application.Interface;

namespace UCDG.Persistence.Repositories
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly UCDGDbContext _context;
        readonly CultureInfo culture = new CultureInfo("en-ZA");

        public PaymentsRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payments>> Add(List<PaymentsViewModel> model)
        {
            try
            {
                List<Payments> savedPayment = new List<Payments>();

                Applications applications = await _context.Applications.FirstOrDefaultAsync(u => u.Id == model[0].ApplicationsId);

                foreach (var paid in model)
                {

                    Payments item = new Payments();

                    item.Type = paid.Type;
                    item.NumberOfWeeks = paid.NumberOfWeeks;
                    item.RatePerHour = paid.RatePerHour;
                    item.HoursPerWeek = paid.HoursPerWeek;
                    item.TotalNumberOfHours = paid.TotalNumberOfHours;
                    item.StartDate = paid.StartDate;
                    item.EndDate = paid.EndDate;
                    item.MonthTotal = Convert.ToDouble(paid.MonthTotal);
                    item.Step = paid.Step;
                    item.Applications = applications;

                    var results = await _context.Payments.AddAsync(item);
                    _context.SaveChanges();
                    if (results != null)
                    {
                        savedPayment.Add(item);
                    }
                }

                await _context.SaveChangesAsync();

                return savedPayment;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<Payments> UpdateCareerDevelopmentPayments(int applicationId)
        {
            try
            {
                Payments payments = new Payments();
                if (applicationId != 0)
                {
                    List<Payments> paymentList = await _context.Payments.Include(f => f.Applications).Where(f => f.Applications.Id == applicationId && f.Step == null).ToListAsync();

                    foreach (var item in paymentList)
                    {
                        item.Step = ApplicationStepsEnum.CareerDevelopment.GetDescription();
                        _context.Payments.Update(item);
                        await _context.SaveChangesAsync();
                        payments = item;
                    }

                }

                return payments;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<Payments> UpdateImprovementOfStaffQualificationsPayments(int applicationId)
        {
            try
            {
                Payments payments = new Payments();
                if (applicationId != 0)
                {
                    List<Payments> paymentList = await _context.Payments.Include(f => f.Applications).Where(f => f.Applications.Id == applicationId && f.Step == null).ToListAsync();

                    foreach (var item in paymentList)
                    {
                        item.Step = ApplicationStepsEnum.ImprovementOfStaffQualifications.GetDescription();
                        _context.Payments.Update(item);
                        await _context.SaveChangesAsync();
                        payments = item;
                    }

                }

                return payments;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<List<PaymentsViewModel>> GetPaymentsByApplicationId(int applicationsId)
        {
            try
            {
                List<PaymentsViewModel> payments = new List<PaymentsViewModel>();

                List<Payments> doc = await _context.Payments.Include(c => c.Applications).Where(u => u.Applications.Id == applicationsId && u.Step.Trim().ToLower() == ApplicationStepsEnum.ImprovementOfStaffQualifications.GetDescription().Trim().ToLower()).ToListAsync();

                foreach (var item in doc)
                {
                    var model = new PaymentsViewModel
                    {
                        Id = item.Id,
                        Type = item.Type,
                        NumberOfWeeks = item.NumberOfWeeks,
                        HoursPerWeek = item.HoursPerWeek,
                        TotalNumberOfHours = item.TotalNumberOfHours,
                        RatePerHour = item.RatePerHour,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        MonthTotal = item.MonthTotal,
                        ApplicationsId = item.Applications.Id,
                        Step = item.Step
                    };

                    payments.Add(model);

                }

                return payments;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }
        }

        public async Task<List<PaymentsViewModel>> GetCareerDevelopmentPaymentsByApplicationId(int applicationsId)
        {
            try
            {
                List<PaymentsViewModel> payments = new List<PaymentsViewModel>();


                List<Payments> doc = await _context.Payments.Include(c => c.Applications).Where(u => u.Applications.Id == applicationsId && u.Step.Trim().ToLower() == ApplicationStepsEnum.CareerDevelopment.GetDescription().Trim().ToLower()).ToListAsync();

                foreach (var item in doc)
                {
                    var model = new PaymentsViewModel
                    {
                        Id = item.Id,
                        Type = item.Type,
                        NumberOfWeeks = item.NumberOfWeeks,
                        HoursPerWeek = item.HoursPerWeek,
                        TotalNumberOfHours = item.TotalNumberOfHours,
                        RatePerHour = item.RatePerHour,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        MonthTotal = item.MonthTotal,
                        ApplicationsId = item.Applications.Id,
                        Step = item.Step
                    };

                    payments.Add(model);

                }

                return payments;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }
        }


        public async Task<int> DeletePayment(int paymentId)
        {
            try
            {
                if (paymentId <= 0)
                    return 0;

                var payment = await _context.Payments.FirstOrDefaultAsync(f => f.Id == paymentId);

                if (payment == null)
                    return 0;

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();

                return paymentId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting payment with id {paymentId}", ex);
            }
        }

        public async Task<Payments> SaveImprovementOfStaffQualificationsPayment(PaymentsViewModel model)
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

                Payments entity;

                if (model.Id > 0)
                {
                    entity = await _context.Payments.Include(x => x.Applications).FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (entity == null)
                    {
                        return null;
                    }
                }
                else
                {
                    entity = new Payments();
                    _context.Payments.Add(entity);
                }

                entity.Applications = application;
                entity.Type = model.Type?.Trim();
                entity.StartDate = model.StartDate;
                entity.EndDate = model.EndDate;
                entity.NumberOfWeeks = model.NumberOfWeeks;
                entity.HoursPerWeek = model.HoursPerWeek;
                entity.TotalNumberOfHours = model.TotalNumberOfHours;
                entity.RatePerHour = model.RatePerHour;
                entity.MonthTotal = model.MonthTotal;
                entity.Step = ApplicationStepsEnum.ImprovementOfStaffQualifications.GetDescription();

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
