using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Users.Resources
{
    public class AssignUserRoleResouce
    {
        public string Username { get; set; }
        public int LoggedInUser { get; set; }
        public bool IsActive { get; set; }
        public List<string> RoleNames { get; set; }
        public string Role { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string[] RefNumber { get; set; }
        public string TempRole { get; set; }
    }
}
