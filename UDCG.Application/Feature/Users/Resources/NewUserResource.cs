using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Feature.Roles.Resources;

namespace UDCG.Application.Feature.Users.Resources
{
    public class NewUserResource
    {
        public string Username { get; set; }
        public string CreatedByUsername { get; set; }
        public List<ReadRoleResource> Roles { get; set; }
        public DateTime ExpiryDate { get; set; }

    }
}
