using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data.Configurations;

/// <summary>
/// Read-only mapping of <c>audit.change_log</c>. The table, function and triggers are created by raw SQL in the
/// AddAuditTrail migration (ADR 0027), so EF never generates DDL for it.
/// </summary>
public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("change_log", "audit", t => t.ExcludeFromMigrations());
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TableName).HasColumnName("table_name");
        builder.Property(e => e.Operation).HasColumnName("operation");
        builder.Property(e => e.RowId).HasColumnName("row_id");
        builder.Property(e => e.OldValues).HasColumnName("old_values").HasColumnType("jsonb");
        builder.Property(e => e.NewValues).HasColumnName("new_values").HasColumnType("jsonb");
        builder.Property(e => e.ChangedFields).HasColumnName("changed_fields");
        builder.Property(e => e.AuthorName).HasColumnName("author_name");
        builder.Property(e => e.FrontendUrl).HasColumnName("frontend_url");
        builder.Property(e => e.RequestId).HasColumnName("request_id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
