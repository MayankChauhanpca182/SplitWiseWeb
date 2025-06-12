using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;

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

    public async Task<T?> Get(Expression<Func<T, bool>>? predicate = null)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<List<T>> List(Expression<Func<T, bool>>? predicate = null)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task Add(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Update(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }
}
