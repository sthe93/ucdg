using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCDG.Infrastructure.Interfaces
{
 public interface IUserIdResolver
{
    Task<IReadOnlyList<int>> ResolveUserIdsAsync(int newUserId);
        Task<IReadOnlyList<int>> ResolveUserIdsAsyncV2(int newUserId);
}
}
