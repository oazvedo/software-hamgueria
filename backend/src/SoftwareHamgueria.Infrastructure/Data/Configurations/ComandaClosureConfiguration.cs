using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwareHamgueria.Application.Validators;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data.Configurations;

public class ComandaClosureConfiguration : IEntityTypeConfiguration<ComandaClosure>
{
    public void Configure(EntityTypeBuilder<ComandaClosure> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // the domain generates ids (spec 024, same fix as OrderItem in 005)
        builder.Property(c => c.CustomerName).IsRequired().HasMaxLength(Limits.CustomerName);
        builder.HasIndex(c => new { c.TableId, c.CustomerName }).IsUnique();
    }
}
