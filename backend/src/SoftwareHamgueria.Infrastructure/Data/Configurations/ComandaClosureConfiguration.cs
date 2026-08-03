using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data.Configurations;

public class ComandaClosureConfiguration : IEntityTypeConfiguration<ComandaClosure>
{
    public void Configure(EntityTypeBuilder<ComandaClosure> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CustomerName).IsRequired().HasMaxLength(60);
        builder.HasIndex(c => new { c.TableId, c.CustomerName }).IsUnique();
    }
}
