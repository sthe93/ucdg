using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class ApplicationsProjects
    {
        public int ApplicationsId { get; set; }
        public Applications Applications { get; set; }

        public int ProjectsId { get; set; }
        public Projects Projects { get; set; }
    }
}
