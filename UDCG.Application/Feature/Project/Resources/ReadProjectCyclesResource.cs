using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UDCG.Application.Feature.Project.Resources
{
  public  class ReadProjectCyclesResource
    {
        [Key]
        public int Id { get; set; }
        public string Period { get; set; }
        public bool IsActive { get; set; }
    }
}
