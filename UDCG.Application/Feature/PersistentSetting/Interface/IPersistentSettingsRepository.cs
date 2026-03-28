using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.PersistentSetting.Interface
{
    public interface IPersistentSettingsRepository
    {
        Task<PersistentSettings> Add(PersistentSettings model);

        Task<PersistentSettings> GetPersistentSetting(string persistentSettingsKey);

        Task<bool> Exists(string persistentSettingsKey);

        Task<PersistentSettings> UpdatePersistentSettings(PersistentSettings persistentSettings);

        Task<string> GetReferenceNumber();
    }
}
