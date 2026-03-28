using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
   public class UserRole
    {
        public int UserRoleId { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime DateModified { get; set; }

        public UserStoreUser? User { get; set; }

    }
}

