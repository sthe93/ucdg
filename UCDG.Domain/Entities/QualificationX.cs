using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
   public class QualificationX
    {
        public int QualificationXId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string InstitutionName { get; set; }
        public string QualificationType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
