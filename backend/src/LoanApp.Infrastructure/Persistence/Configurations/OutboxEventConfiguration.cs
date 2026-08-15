using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanApp.Infrastructure.Persistence.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("OutboxEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Payload)
            .IsRequired();

        builder.Property(e => e.Operation)
            .HasConversion<string>().HasMaxLength(20);
            
        builder.Property(e => e.Status)
            .HasConversion<string>().HasMaxLength(20);

        // El BackgroundService hace polling filtrando por Status,
        // esto evita table scan.
        builder.HasIndex(e => e.Status);
    }
}