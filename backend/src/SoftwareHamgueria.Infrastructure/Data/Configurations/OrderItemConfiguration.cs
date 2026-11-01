using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwareHamgueria.Application.Validators;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        // The domain generates the key (OrderItem ctor). Without this, EF takes a new item added to a tracked order
        // for an existing row and updates instead of inserting (PUT /api/order failed with 0 rows affected).
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(Limits.ItemNotes);
        builder.HasOne(i => i.Dish).WithMany().HasForeignKey(i => i.DishId).OnDelete(DeleteBehavior.Restrict);
    }
}
