using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCDG.Persistence.Enums
{
   public enum EmployeePersonTypeEnum
    {
        [Description("Executive Management")]
        ExecutiveManagement = 1,

        [Description("Vice Dean")]
        ViceDean = 2,
            
        [Description("Executive Director")]
        ExecutiveDirector = 3,

        [Description("Director")]
        Director = 4,

       
        
    }
}
