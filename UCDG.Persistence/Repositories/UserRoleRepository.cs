using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Persistence.Repositories
{
    public class UserRoleRepository
    {
        private readonly UCDGDbContext _context;

        public UserRoleRepository(UCDGDbContext context)
        {
            _context = context;
        }

    }
}
