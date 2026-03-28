using System.Collections.Generic;
using System.Threading.Tasks;


namespace UDCG.Application.Feature.Comments.Inteface
{
    public interface ICommentRepository
    {
        Task<List<UCDG.Domain.Entities.Comments>> GetAllComment();
      
        Task<List<UCDG.Domain.Entities.Comments>> GetApplicationCommentByUserId(int userId);
    }
}





     

