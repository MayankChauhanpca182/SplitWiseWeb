using System.Linq.Expressions;
using SplitWiseRepository.ViewModels;

namespace SplitWiseRepository.Repositories.Interface;

public interface IGenericRepository<T> where T : class
{
    public Task<T> Get(
        Expression<Func<T, bool>> predicate = null,
        List<Expression<Func<T, object>>> includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>> thenIncludes = null
    );

    public Task<bool> Any(Expression<Func<T, bool>> predicate = null);

    public Task<List<T>> List(
        Expression<Func<T, bool>> predicate = null,
        List<Expression<Func<T, object>>> includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>> thenIncludes = null
    );

    public Task<PaginatedItemsVM<T>> PaginatedList(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        List<Expression<Func<T, object>>> includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>> thenIncludes = null,
        int? pageSize = null,
        int? pageNumber = null,
        IQueryable<T> sourceQuery = null
    );

    public Task<T> Add(T entity);
    public Task<T> Update(T entity);
    public IQueryable<T> Query();

    public Task<int> Count(
        Expression<Func<T, bool>> predicate = null,
        List<Expression<Func<T, object>>> includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>> thenIncludes = null
    );

    public Task<decimal> Sum(
        Expression<Func<T, decimal>> selector,
        Expression<Func<T, bool>> predicate = null,
        List<Expression<Func<T, object>>> includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>> thenIncludes = null
    );
}
