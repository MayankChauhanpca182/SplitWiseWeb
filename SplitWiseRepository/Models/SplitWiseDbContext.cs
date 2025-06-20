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
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public virtual DbSet<ExceptionLog> ExceptionLogs { get; set; }
    public virtual DbSet<Currency> Currencies { get; set; }
    public virtual DbSet<UserReferral> UserReferrals { get; set; }
    public virtual DbSet<Friend> Friends { get; set; }
    public virtual DbSet<FriendRequest> FriendRequests { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<GroupMember> GroupMembers { get; set; }
}
