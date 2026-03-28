using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using UDCG.Application.Feature.PersistentSetting.Interface;
using UCDG.Domain.Entities;
using UCDG.Persistence.Enums;
using System.Threading.Tasks;
using System.Linq;

namespace UCDG.Persistence.Repositories
{
    public class PersistentSettingsRepository : IPersistentSettingsRepository
    {
        private readonly UCDGDbContext _context;

        int currentYear = 0;
        int referenceNumberCount = 0;

        public PersistentSettingsRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<PersistentSettings> Add(PersistentSettings model)
        {
            try
            {
                var results = await _context.PersistentSettings.AddAsync(model);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.ToString());
             
            }

            return model;
        }

        public async Task<PersistentSettings> GetPersistentSetting(string persistentSettingsKey)
        {
            var persistentSettings = new PersistentSettings();

            persistentSettings = await _context.PersistentSettings.FirstOrDefaultAsync(ps => ps.PersistentSettingsKey == persistentSettingsKey);
            return persistentSettings;
        }

        public async Task<bool> Exists(string persistentSettingsKey)
        {
            var result = await _context.PersistentSettings.FirstOrDefaultAsync(ps => ps.PersistentSettingsKey == persistentSettingsKey) != null;
            return result;
        }

        public async Task<PersistentSettings> UpdatePersistentSettings(PersistentSettings persistentSettings)
        {
            var entity = await _context.PersistentSettings.FirstAsync(ps => ps.PersistentSettingsKey == persistentSettings.PersistentSettingsKey);

            if (entity != null)
            {
                entity.PersistentSettingsValue = persistentSettings.PersistentSettingsValue;
                persistentSettings.PersistentSettingsId = entity.PersistentSettingsId;

                await _context.SaveChangesAsync();
            }

            return persistentSettings;

        }

        public async Task<string> GetReferenceNumber()
        {
            try
            {
                string referenceNumber = "";
                
                var currentYearExists = Exists(PersistentSettingsEnum.CurrentYear.ToString());

                if (!currentYearExists.Result)
                {
                    currentYear = CreateCurrentYearPersistentSetting().Result;
                }
                else
                {
                    // IF CurrentYear SETTING EXISTS, RETRIEVE.
                    currentYear = await GetCurrentYearPersistentSetting();
                }

                var referenceNumberCountExists = Exists(PersistentSettingsEnum.ReferenceNumberCount.ToString());

                if (!referenceNumberCountExists.Result)
                {
                    referenceNumberCount = CreateReferenceNumberCountSetting().Result;
                }
                else
                {
                    // IF ReferenceNumberCount SETTING EXIST, RETRIEVE.
                    PersistentSettings persistentSetting = GetPersistentSetting(PersistentSettingsEnum.ReferenceNumberCount.ToString()).Result;
                    referenceNumberCount = persistentSetting.PersistentSettingsValue;
                }

                referenceNumber = CreateApplicationReferenceNumber(currentYear, referenceNumberCount);

                await UpdateReferenceNumberCountSetting(++referenceNumberCount);

               

                return referenceNumber;
            }
            catch(Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }
            
        }

        private string CreateApplicationReferenceNumber(int year, int refNumberCount)
        {
            int length = 5 - refNumberCount.ToString().Length;

            var addChar = "";
            for (int i = 0; i < length; i++)
            {
                addChar = addChar + "0";
            }

            return currentYear + addChar + referenceNumberCount;
        }

        private async Task<int> CreateReferenceNumberCountSetting()
        {
            PersistentSettings persistentSetting = new PersistentSettings()
            {
                PersistentSettingsKey   = PersistentSettingsEnum.ReferenceNumberCount.ToString(),
                PersistentSettingsValue = 1
            };
            await Add(persistentSetting);
            return persistentSetting.PersistentSettingsValue;
        }

        private async Task<int> GetCurrentYearPersistentSetting()
        {          
            PersistentSettings persistentSetting = GetPersistentSetting(PersistentSettingsEnum.CurrentYear.ToString()).Result;
            currentYear = persistentSetting.PersistentSettingsValue;

            // IF !CurrentYear, UPDATE.
            if (currentYear != 0 && currentYear != DateTime.Now.Year)
            {
                currentYear = DateTime.Now.Year;
                persistentSetting = new PersistentSettings()
                {
                    PersistentSettingsKey   = PersistentSettingsEnum.CurrentYear.ToString(),
                    PersistentSettingsValue = currentYear
                };
                
                await UpdatePersistentSettings(persistentSetting);

                await UpdateReferenceNumberCountSetting(1);
            }

            return currentYear;
        }

        private async Task<PersistentSettings> UpdateReferenceNumberCountSetting(int newReferenceNumberCount)
        {
            PersistentSettings persistentSetting = new PersistentSettings()
            {
                PersistentSettingsKey   = PersistentSettingsEnum.ReferenceNumberCount.ToString(),
                PersistentSettingsValue = newReferenceNumberCount
            };
            return await UpdatePersistentSettings(persistentSetting);
        }

        private async Task<int> CreateCurrentYearPersistentSetting()
        {
            PersistentSettings persistentSetting = new PersistentSettings()
            {
                PersistentSettingsKey = PersistentSettingsEnum.CurrentYear.ToString(),
                PersistentSettingsValue = DateTime.Now.Year
            };

            await Add(persistentSetting);
            return persistentSetting.PersistentSettingsValue;
        }
    }
}
