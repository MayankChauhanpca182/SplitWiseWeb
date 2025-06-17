using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;

namespace SplitWiseRepository.Repositories.Implementation;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly SplitWiseDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(SplitWiseDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T> Get(Expression<Func<T, bool>> predicate = null)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<T> GetLast(Expression<Func<T, bool>> predicate = null)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<List<T>> List(Expression<Func<T, bool>> predicate = null)
    {
        if (predicate != null)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        return _dbSet.ToList();
    }

    public async Task<PaginatedItemsVM<T>> PaginatedList(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        int? pageSize = null,
        int? pageNumber = null)
    {

        IQueryable<T> query = _dbSet;
        PaginatedItemsVM<T> paginatedItems = new PaginatedItemsVM<T>();

        //Apply Filters
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        //Order By
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // 
        if (pageSize != null && pageSize != null)
        {
            paginatedItems.totalRecords = await query.CountAsync();
            paginatedItems.Items = await query.Skip((int)((pageNumber - 1) * pageSize)).Take((int)pageSize).ToListAsync();
        }

        return paginatedItems;
    }

    public async Task<T> Add(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> Update(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}
