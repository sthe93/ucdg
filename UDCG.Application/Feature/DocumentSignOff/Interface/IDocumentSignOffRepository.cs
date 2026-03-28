using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.DocumentSignOff.Resources;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface IDocumentSignOffRepository
    {
        //get by refference
        Task<(Applications applications, UCDG.Domain.Entities.UserStoreUser user)> GetMyApplicationsByReff(string reff);
        Task<DocumentSignOffs> AddDocumentSignOff(ReadDocumentSignOffViewModel model);

        //  Task<Applications> GetAppliocationWithSignature(string reff, int Id, string Role, int UserId); //UserRoleName 
        Task<DocumentSignOffs> GetConfirmationSignature(string reff, string Role, int UserId, int Id);
        Task<UCDG.Domain.Entities.UserStoreUser> GetSiadirectorsDetails();
        bool IsSigned(string refferenceNumber, int applicationId, int userId);
    }
}
