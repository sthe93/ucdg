using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.SupportRequired.Interface;

namespace UCDG.Persistence.Repositories
{
    public class ApplicationsSupportRequiredRepository : IApplicationSupportRequiredRepository
    {
        private readonly UCDGDbContext _context;

        public ApplicationsSupportRequiredRepository(UCDGDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicationSupportRequired>> GetApplicationsSupportRequiredById(int applicationId)
        {
            try
            {
                if (applicationId != 0)
                {
                    List<ApplicationSupportRequired> supportRequired = await _context.ApplicationSupportRequired.Where(u => u.ApplicationsId == applicationId).ToListAsync();

                    return supportRequired;

                }

                return null;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }
    }
}
