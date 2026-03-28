using UCDG.Domain.Entities;
using System.Threading.Tasks;

namespace UDCG.Application.Feature.Application.Interface
{
    public interface IUserStoreUserRepository
    {
        Task<UserStoreUser> GetUserStoreUserByUsername(string username);
    }
}
