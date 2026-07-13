using MoneyFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyFlow.Infrastructure.Persistence.Configurations;

public sealed class TransactionTagConfiguration : IEntityTypeConfiguration<TransactionTag>
{
    public void Configure(EntityTypeBuilder<TransactionTag> builder)
    {
        builder.ToTable("transaction_tags");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionId).IsRequired();
        builder.Property(x => x.TagId).IsRequired();

        builder.HasIndex(x => new { x.TransactionId, x.TagId })
            .IsUnique();

        builder.HasIndex(x => x.TagId);
    }
}
