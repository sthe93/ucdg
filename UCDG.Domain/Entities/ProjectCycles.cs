using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class ProjectCycles
    {
        [Key]
        public int Id { get; set; }
        public string Period { get; set; }
        public bool IsActive { get; set; }
    }
}
