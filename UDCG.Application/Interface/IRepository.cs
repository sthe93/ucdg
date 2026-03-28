using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UDCG.Application.Interface
{
    public interface IRepository<TEntity> where TEntity : class
    {

        Task AddAsync(TEntity entity);
        Task AddRangeAsync(List<TEntity> entity);
        Task<List<TEntity>> GetAsync();
        Task<TEntity> GetByIdAsync(int id);
        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
