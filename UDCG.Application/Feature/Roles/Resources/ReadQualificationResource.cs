using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Roles.Resources
{
   public class ReadQualificationResource
    {
        public int QualificationId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string InstitutionName { get; set; }
        public string QualificationType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
