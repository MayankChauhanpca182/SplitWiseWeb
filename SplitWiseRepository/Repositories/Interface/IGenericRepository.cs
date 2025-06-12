using System.Linq.Expressions;

namespace SplitWiseRepository.Repositories.Interface;

public interface IGenericRepository<T> where T : class
{
    public Task<T?> Get(Expression<Func<T, bool>>? predicate = null);
    public Task<List<T>> List(Expression<Func<T, bool>>? predicate = null);
    public Task<T> Add(T entity);
    public Task<T> Update(T entity);
}
