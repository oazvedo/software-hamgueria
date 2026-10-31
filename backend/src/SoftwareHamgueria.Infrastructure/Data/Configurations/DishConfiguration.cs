using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwareHamgueria.Application.Validators;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data.Configurations;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(Limits.DishName);
        builder.Property(d => d.Description).HasMaxLength(Limits.DishDescription);
        // Money (spec 016): exact decimal, 2 places; never double.
        builder.Property(d => d.Price).IsRequired().HasPrecision(10, 2);
        builder.Property(d => d.Category).IsRequired();
        builder.Property(d => d.IsActive).HasDefaultValue(true);
    }
}
