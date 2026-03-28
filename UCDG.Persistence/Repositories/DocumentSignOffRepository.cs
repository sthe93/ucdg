using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
//look at the role interface 

using UCDG.Domain.Entities;
using UCDG.Persistence.Enums;
using UDCG.Application.Feature.Application.Interface;
using UDCG.Application.Feature.DocumentSignOff.Resources;
using System;
using UDCG.Application.Common.Constants;
using UDCG.Application.Feature.Roles.Resources;

namespace UCDG.Persistence.Repositories
{
    public class DocumentSignOffRepository : IDocumentSignOffRepository
    {
        private readonly UCDGDbContext _context;
        private readonly UserStoreDbContext _userStoreDbContext;

        public DocumentSignOffRepository(UCDGDbContext context, UserStoreDbContext userStoreDbContext)
        {
            _context = context;
            _userStoreDbContext = userStoreDbContext;
        }

        public async Task<DocumentSignOffs> AddDocumentSignOff(ReadDocumentSignOffViewModel model)
        {
            try
            {
                DocumentSignOffs docs = await _context.DocumentSignOffs.FirstOrDefaultAsync(u => u.ReferenceNumber == model.ReferenceNumber);
                if (docs == null)
                {

                    DocumentSignOffs documenSignOffs = new DocumentSignOffs();
                    documenSignOffs.DocumentType = model.DocumentType;
                    documenSignOffs.UserId = model.UserId;
                    documenSignOffs.UserFullName = model.UserFullName;
                    documenSignOffs.SignedDate = model.SignedDate;
                    documenSignOffs.UserRoleName = model.UserRoleName;
                    documenSignOffs.ReferenceNumber = model.ReferenceNumber;
                    documenSignOffs.ApplicationsId = model.ApplicationsId;


                    var results = await _context.DocumentSignOffs.AddAsync(documenSignOffs);
                    await _context.SaveChangesAsync();

                    return documenSignOffs;
                }
                return null;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        ////this works 
        public async Task<List<Applications>> GetMyApplicationsByReff2(string reff)
        {
            var res = await _context.Applications
                .Include(o => o.ApplicationStatus)
                .Include(o => o.Applicant)
                .Include(o => o.FundingCalls)

               .Where(o => o.ReferenceNumber == reff && o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.Approved.GetDescription().ToLower().Trim()).ToListAsync();
            return res;

            //  Applications application = await _context.Applications.Include(c => c.User).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.Id == id );
        }
        //  get applications by ID
        public async Task<(Applications, UserStoreUser)> GetMyApplicationsByReff(string reff)
        {
            try
            {
                if (reff != null)
                {
                    Applications application = await _context.Applications.Include(c => c.Applicant).Include(c => c.FundingCalls).Include(c => c.ApplicationStatus).FirstOrDefaultAsync(f => f.ReferenceNumber == reff);
                    //var resualt = IsSigned()
                    if (application.ApplicationStatus.ApplicationStatusId == (int)ApplicationStatusEnum.ApprovedbySIADirector)
                    {
                        var siaUser = await (from user in _userStoreDbContext.Users
                                             join userrole in _userStoreDbContext.UserRoles.Where(a => a.RoleId == (int)RolesEnum.SIA_Director && a.IsActive == true)
                                             on user.UserId equals userrole.UserId
                                             select new
                                             {
                                                 user.Surname,
                                                 user.Title,
                                                 user.Name
                                             }).FirstOrDefaultAsync();
                        return (application, new UserStoreUser() { Surname = siaUser.Surname, Name = siaUser.Name });
                    }
                    return (application, null);

                }
                return (null, null);

            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<DocumentSignOffs> GetAppliocationWithSignature(string reff, int Id, string Role, int UserId)
        {
            if (reff != null && Id > 0 && Role != null && UserId > 0)
            {
                DocumentSignOffs sing = await _context.DocumentSignOffs
                     .FirstOrDefaultAsync(u => u.UserId == UserId && u.ReferenceNumber == reff && u.ApplicationsId == Id);

                return sing;

            }
            return null;
        }

        public bool IsSigned(string refferenceNumber, int applicationId, int userId)
        {

            DocumentSignOffs sign = _context.DocumentSignOffs
           .FirstOrDefault(u => u.UserId == userId && u.ReferenceNumber == refferenceNumber && u.ApplicationsId == applicationId);

            return sign != null;

        }


        public async Task<DocumentSignOffs> GetConfirmationSignature(string reff, string Role, int UserId, int Id)
        {
            try
            {
                if (reff != null && UserId > 0 && Role != null && Id > 0)
                {
                    DocumentSignOffs application = await _context.DocumentSignOffs
                        .Include(c => c.Applications)
                       .FirstOrDefaultAsync(c => c.ReferenceNumber == reff && c.UserId == UserId && c.UserRoleName == Role && c.ApplicationsId == Id);

                    return application;
                }
                return null;

            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }


        //still Need to debug here 
        public async Task<UserStoreUser> GetSiadirectorsDetails()
        {
            try
            {

                UserStoreUser user = await _userStoreDbContext.Users.Include(r => r.UserRoles).FirstOrDefaultAsync(r => r.Username == UserRolesConstants.SIADirector);
                return user;

            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }


        }



    }
}



