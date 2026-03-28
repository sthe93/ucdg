using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application.Interface;

namespace UCDG.Persistence.Repositories
{
    public class UserStoreUserRepository: IUserStoreUserRepository
    {
        private readonly UserStoreDbContext _userStore;
        public UserStoreUserRepository(UserStoreDbContext userStore)
        {
            _userStore = userStore;
        }
        public async Task<UserStoreUser> GetUserStoreUserByUsername(string username)
        {
            var user = this._userStore.Users.AsNoTracking().Where(x => x.Username == username).FirstOrDefault();
            return user;
        }
    }
}
