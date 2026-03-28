using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCDG.Domain.Entities
{
    public class MecMembers
    {
        public int Id { get; set; }
        public string EmployeeNo { get; set; }
        public string Title { get; set; }
        public string Initials { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
    }
}
