using Microsoft.EntityFrameworkCore;

namespace SplitWiseRepository.Models;

public class SplitWiseDbContext : DbContext
{
    protected SplitWiseDbContext()
    {
    }

    public SplitWiseDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
}
