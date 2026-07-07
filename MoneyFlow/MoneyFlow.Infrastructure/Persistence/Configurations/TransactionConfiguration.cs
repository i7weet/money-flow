using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyFlow.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.OwnsOne(x => x.Amount, owned =>
        {
            owned.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(19, 4)
                .IsRequired();

            owned.Property(x => x.CurrencyCode)
                .HasColumnName("currency_code")
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();
        });

        builder.OwnsOne(x => x.CounterAmount, owned =>
        {
            owned.Property(x => x.Amount)
                .HasColumnName("counter_amount")
                .HasPrecision(19, 4);

            owned.Property(x => x.CurrencyCode)
                .HasColumnName("counter_currency_code")
                .HasMaxLength(3)
                .IsFixedLength();
        });

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(19, 8);

        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
        builder.HasIndex(x => x.PrimaryAccountId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.UserId);
    }
}
