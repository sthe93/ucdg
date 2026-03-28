using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCDG.Infrastructure.Interfaces;
using UCDG.Persistence;

namespace UCDG.Infrastructure.Helpers
{
    public class UserIdResolver : IUserIdResolver
    {
        private readonly UCDGDbContext _ucdp;
        private readonly UserStoreDbContext _userStore;

        public UserIdResolver(
            UCDGDbContext ucdp,
            UserStoreDbContext userStore)
        {
            _ucdp = ucdp;
            _userStore = userStore;
        }

    public async Task<IReadOnlyList<int>> ResolveUserIdsAsync(int newUserId)
{
    try
    {
        // Try async (works with real EF Core providers)
        var staffNumber = await _userStore.Users
            .AsNoTracking()
            .Where(u => u.UserId == newUserId)
            .Select(u => u.HRPostNumber)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(staffNumber))
            return new[] { newUserId };

        var normalized = staffNumber.Trim().ToLower();

        var oldIds = await _ucdp.Users
            .AsNoTracking()
            .Where(u => u.StaffNumber != null && u.StaffNumber.Trim().ToLower() == normalized)
            .Select(u => u.UserId)
            .ToListAsync();

        return new[] { newUserId }.Concat(oldIds).Distinct().ToList();
    }
    catch (InvalidOperationException)
    {
        // Fallback path for providers without async (e.g., unit tests with LINQ-to-Objects)
        var username = _userStore.Users
            .AsNoTracking()
            .Where(u => u.UserId == newUserId)
            .Select(u => u.Username)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(username))
            return new[] { newUserId };

        var normalized = username.Trim().ToLower();

        var oldIds = _ucdp.Users
            .AsNoTracking()
            .Where(u => u.StaffNumber != null && u.StaffNumber.Trim().ToLower() == normalized)
            .Select(u => u.UserId)
            .ToList();

        return new[] { newUserId }.Concat(oldIds).Distinct().ToList();
    }
}



 public async Task<IReadOnlyList<int>> ResolveUserIdsAsyncV2(int newUserId)
{
    try
    {
        // Async path (real EF Core providers)
        var staffNumber = await _userStore.Users
            .AsNoTracking()
            .Where(u => u.UserId == newUserId)
            .Select(u => u.HRPostNumber)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(staffNumber))
            return new[] { newUserId };

        var normalized = staffNumber.Trim().ToLowerInvariant();

        var oldIds = await _ucdp.Users
            .AsNoTracking()
            .Where(u => u.StaffNumber != null && u.StaffNumber.Trim().ToLower() == normalized)
            .Select(u => u.UserId)
            .ToListAsync()
            .ConfigureAwait(false);

        return new[] { newUserId }.Concat(oldIds).Distinct().ToList();
    }
    catch (InvalidOperationException)
    {
        // Fallback path for providers without async (e.g., unit tests with LINQ-to-Objects)
        var staffNumber = _userStore.Users
            .AsNoTracking()
            .Where(u => u.UserId == newUserId)
            .Select(u => u.HRPostNumber) // <-- FIX: get staff number, not username
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(staffNumber))
            return new[] { newUserId };

        //var normalized = staffNumber.;

        var oldIds = _ucdp.Users
            .AsNoTracking()
            .Where(u => u.StaffNumber != null && u.StaffNumber.Trim().ToLower() == staffNumber)
            .Select(u => u.UserId)
            .ToList();

        return new[] { newUserId }.Concat(oldIds).Distinct().ToList();
    }
}
    }
}

