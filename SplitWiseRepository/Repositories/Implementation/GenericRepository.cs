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

    public async Task<bool> Any(Expression<Func<T, bool>> predicate = null)
    {
        return await _dbSet.AnyAsync(predicate);
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
        List<Expression<Func<T, object>>> includes = null,
        List<Func<IQueryable<T>, IQueryable<T>>> thenIncludes = null,
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

        // Apply Includes (First-level navigation properties)
        if (includes != null)
        {
            foreach (Expression<Func<T, object>> include in includes)
            {
                query = query.Include(include);
            }
        }

        // Apply ThenIncludes (Deeper navigation properties)
        if (thenIncludes != null)
        {
            foreach (var thenInclude in thenIncludes)
            {
                query = thenInclude(query);
            }
        }

        // Set total records
        paginatedItems.totalRecords = await query.CountAsync();

        // Apply pagination
        if (pageSize != null && pageNumber != null)
        {
            paginatedItems.Items = await query.Skip((int)((pageNumber - 1) * pageSize)).Take((int)pageSize).ToListAsync();
        }
        else
        {
            paginatedItems.Items = await query.ToListAsync();
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
