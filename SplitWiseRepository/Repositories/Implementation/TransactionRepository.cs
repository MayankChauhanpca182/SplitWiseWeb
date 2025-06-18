using Microsoft.EntityFrameworkCore.Storage;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;

namespace SplitWiseRepository.Repositories.Implementation;

public class TransactionRepository : ITransactionRepository
{
    private readonly SplitWiseDbContext _context;
    private IDbContextTransaction? _transaction;

    public TransactionRepository(SplitWiseDbContext context)
    {
        _context = context;
    }

    public async Task Begin()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task Commit()
    {
        if (_transaction != null)
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task Rollback()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
