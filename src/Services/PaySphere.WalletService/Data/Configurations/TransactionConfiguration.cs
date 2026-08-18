using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaySphere.WalletService.Entities;

namespace PaySphere.WalletService.Data.Configurations;

/// <summary>
/// EF Core configuration for Transaction entity. Ensures currency fields use
/// decimal(18,2), configures indexes and length limits for reference and description.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WalletId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.BalanceBefore)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.BalanceAfter)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Reference)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Reference);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
