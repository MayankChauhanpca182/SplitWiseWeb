using System.Linq.Expressions;
using SplitWiseRepository.ViewModels;

namespace SplitWiseRepository.Repositories.Interface;

public interface IGenericRepository<T> where T : class
{
    public Task<T> Get(Expression<Func<T, bool>> predicate = null);
    public Task<T> GetLast(Expression<Func<T, bool>> predicate = null);
    public Task<bool> Any(Expression<Func<T, bool>> predicate = null);
    public Task<List<T>> List(Expression<Func<T, bool>> predicate = null);
    public Task<PaginatedItemsVM<T>> PaginatedList(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        int? pageSize = null,
        int? pageNumber = null);
    public Task<T> Add(T entity);
    public Task<T> Update(T entity);
}
