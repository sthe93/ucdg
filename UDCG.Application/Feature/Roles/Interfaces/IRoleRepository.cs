using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.Roles.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetRoles();
        Task<List<Role>> GetAssignableUserRoles();

        Task<List<Role>> GetTempAssignableUserRoles(); 
        Task<Role> GetCurrentTempUserRole(string username); 

    }
}
