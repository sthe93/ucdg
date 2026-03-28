using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class PersistentSettings
    {
        public int PersistentSettingsId { get; set; }
        public string PersistentSettingsKey { get; set; }
        public int PersistentSettingsValue { get; set; }
    }
}
