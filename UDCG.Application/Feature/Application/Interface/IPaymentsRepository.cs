using System.Collections.Generic;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface IPaymentsRepository
    {
        Task<List<Payments>> Add(List<PaymentsViewModel> model);
        Task<Payments> UpdateImprovementOfStaffQualificationsPayments(int applicationId);
        Task<Payments> UpdateCareerDevelopmentPayments(int applicationId);
        Task<List<PaymentsViewModel>> GetPaymentsByApplicationId(int applicationsId);
        Task<List<PaymentsViewModel>> GetCareerDevelopmentPaymentsByApplicationId(int applicationsId);
        Task<int> DeletePayment(int paymentId);
        Task<Payments> SaveImprovementOfStaffQualificationsPayment(PaymentsViewModel model);
    }
}
