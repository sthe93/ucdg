using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UDCG.Application.Feature.Roles.Interfaces;
using UCDG.Domain.Entities;
using UCDG.Persistence.Enums;

namespace UCDG.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly UCDGDbContext _context;
        private readonly UserStoreDbContext _userStoreDbContext;

        public RoleRepository(UCDGDbContext context, UserStoreDbContext userStoreDbContext)
        {
            _context = context;
            _userStoreDbContext = userStoreDbContext;
        }

        public async Task<List<Role>> GetRoles()
        {
            var results = await _userStoreDbContext.Roles.ToListAsync();
            return results;
        }

        public async Task<List<Role>> GetAssignableUserRoles()
        {
            List<Role> results = await _userStoreDbContext.Roles.Where(u => u.Name == AssignableUserRoles.FundAdministrator.GetDescription() || 
                                                                 u.Name == AssignableUserRoles.SIADirector.GetDescription() ||
                                                                  u.Name == AssignableUserRoles.Applicant.GetDescription() ||
                                                                 u.Name == AssignableUserRoles.FinancialBusinessPartner.GetDescription()||
                                                                 u.Name == AssignableUserRoles.TemporaryApprover.GetDescription()).ToListAsync();
            return results;
        }

        public async Task<List<Role>> GetTempAssignableUserRoles()
        {
            List<Role> results = await _userStoreDbContext.Roles.Where(u => u.Name == AssignableUserRoles.FundAdministrator.GetDescription() ||
                                                                 u.Name == AssignableUserRoles.SIADirector.GetDescription() ||
                                                                  u.Name == AssignableUserRoles.HOD.GetDescription() ||
                                                                 u.Name == AssignableUserRoles.ViceDean.GetDescription()).ToListAsync();
            return results;
        }

        public async Task<Role> GetCurrentTempUserRole(string username)
        {
            Role role = new Role();

            TemporaryApproverApplications results = await _context.TemporaryApproverApplications.Include(o=>o.User).FirstOrDefaultAsync(o=>o.User.Username== username);
            if (results != null)
            {
                role = await _userStoreDbContext.Roles.Where(u => u.Name == results.ApprovedAs).FirstOrDefaultAsync();
            }
            return role;
        }
    }


}
