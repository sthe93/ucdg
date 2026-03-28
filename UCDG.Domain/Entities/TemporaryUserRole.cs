using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCDG.Domain.Entities
{
    public class TemporaryUserRole
    {
        public int TemporaryUserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ApplicationId { get; set; }

        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public bool IsActive { get; set; }

        public UserStoreUser User { get; set; }
    }
}
