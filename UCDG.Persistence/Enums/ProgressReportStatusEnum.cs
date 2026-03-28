using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UCDG.Persistence.Enums
{
    public enum ProgressReportStatusEnum
    {
        [Description("New")]
        New = 1,

        [Description("Finalized")]
        Finalize = 2,

        [Description("RFI")]
        RFI = 3
    }
}
