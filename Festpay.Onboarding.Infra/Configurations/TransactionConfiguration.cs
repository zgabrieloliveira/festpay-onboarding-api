using Festpay.Onboarding.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Festpay.Onboarding.Infra.Configurations;

public class TransactionConfiguration : ConfigurationBase<Transaction>, IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        ConfigureEntityBase(builder);

        builder.Property(t => t.SourceAccountId).IsRequired();
        builder.Property(t => t.DestinationAccountId).IsRequired();
        builder.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(t => t.IsCancelled).IsRequired();
    }
}