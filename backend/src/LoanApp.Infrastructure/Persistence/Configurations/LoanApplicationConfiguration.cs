using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanApp.Infrastructure.Persistence.Configurations;

public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("Applications");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.RequestedAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.CustomerId)
            .IsRequired();

        // Un cliente tiene una sola aplicación activa (spec: "same SSN means
        // one customer and one application"), por eso el FK también es único.

        builder.HasIndex(a => a.CustomerId)
            .IsUnique();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}