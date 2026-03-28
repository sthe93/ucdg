using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Feature.Roles.Resources;

namespace UDCG.Application.Feature.Users.Resources
{
   public class UserRoleResource
    {

        public int UserRoleId { get; set; }
        public int RoleId { get; set; }
        public ReadRoleResource Role { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime DateModified { get; set; }
    }
}
