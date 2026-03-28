using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public int AppId { get; set; }
        public string RoleType { get; set; } = "";
        public bool IsActive { get; set; }
        public bool? IsTemporary { get; set; }
        public bool IsAssignable { get; set; }
    }
}
