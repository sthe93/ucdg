using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UCDG.Persistence.Enums
{
    public enum PersistentSettingsEnum
    {
        [Description("Current Year")]
        CurrentYear = 1,

        [Description("Application Reference Number Count")]
        ReferenceNumberCount = 2,
    }
}
