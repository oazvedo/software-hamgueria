using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwareHamgueria.Application.Validators;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever(); // the domain generates ids (spec 024, same fix as OrderItem in 005)
        builder.Property(t => t.TableName).IsRequired().HasMaxLength(Limits.TableName);
        builder.Property(t => t.IsActive).HasDefaultValue(true);
    }
}
