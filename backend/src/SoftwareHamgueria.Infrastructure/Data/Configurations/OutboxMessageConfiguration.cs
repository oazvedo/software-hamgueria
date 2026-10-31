using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.RoutingKey).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Payload).IsRequired().HasMaxLength(4000);
        builder.Property(m => m.LastError).HasMaxLength(500);
        // The dispatcher reads unsent messages oldest first.
        builder.HasIndex(m => new { m.SentAt, m.CreatedAt });
    }
}
