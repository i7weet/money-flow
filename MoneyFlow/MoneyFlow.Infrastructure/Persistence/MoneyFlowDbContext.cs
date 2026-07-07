using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MoneyFlow.Infrastructure.Persistence;

public sealed class MoneyFlowDbContext : DbContext
{
    public MoneyFlowDbContext(DbContextOptions<MoneyFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<TransactionTag> TransactionTags => Set<TransactionTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoneyFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
