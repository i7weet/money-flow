using MoneyFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MoneyFlow.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ArchivedAt);

        builder.OwnsOne(x => x.OpeningBalance, owned =>
        {
            owned.Property(x => x.Amount)
                .HasColumnName("opening_balance_amount")
                .HasPrecision(19, 4)
                .IsRequired();

            owned.Property(x => x.CurrencyCode)
                .HasColumnName("opening_balance_currency_code")
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();
        });

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique();

        builder.HasIndex(x => x.UserId);
    }
}
