
using UDCG.Application.Common;
using UDCG.Application.Feature.Resrouces;

namespace UDCG.Application.Interface
{
    public interface IUserService
    {
        public ApiInfoModel ApiInfo { get; set; }
        UserStoreDto GetUserByUsername(string username);
    }
}
