using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanApp.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.State)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Ssn)
            .IsRequired()
            .HasMaxLength(11);

        // Clave de identidad pare el flujo de returnong customer.
        builder.HasIndex(c => c.Ssn)
            .IsUnique();
    }
}