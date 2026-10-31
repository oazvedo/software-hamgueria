using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftwareHamgueria.Infrastructure.Migrations
{
    /// <summary>
    /// Database audit trail by triggers (ADR 0027), after "PostgreSQL triggers for audit" (OneUptime, 2026-01-30):
    /// one generic function writes before/after JSONB, the changed columns, and the author, screen and trace id the
    /// application puts in the session (<c>app.author_name</c>, <c>app.frontend_url</c>, <c>app.request_id</c>).
    /// </summary>
    public partial class AddAuditTrail : Migration
    {
        private static readonly string[] Audited = ["Orders", "OrderItems", "Dishes", "Tables", "ComandaClosures"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SCHEMA IF NOT EXISTS audit;

                CREATE TABLE audit.change_log (
                    id             BIGSERIAL PRIMARY KEY,
                    table_name     TEXT        NOT NULL,
                    operation      TEXT        NOT NULL CHECK (operation IN ('INSERT', 'UPDATE', 'DELETE')),
                    row_id         TEXT,
                    old_values     JSONB,
                    new_values     JSONB,
                    changed_fields TEXT[]      NOT NULL DEFAULT '{}',
                    transaction_id BIGINT      NOT NULL DEFAULT txid_current(),
                    author_name    TEXT,
                    frontend_url   TEXT,
                    request_id     TEXT,
                    db_user        TEXT        NOT NULL DEFAULT session_user,
                    created_at     TIMESTAMPTZ NOT NULL DEFAULT clock_timestamp()
                );

                CREATE INDEX ix_change_log_table_row ON audit.change_log (table_name, row_id);
                CREATE INDEX ix_change_log_created_at ON audit.change_log (created_at);

                -- Generic row trigger. Trigger arguments name columns to ignore when deciding whether an UPDATE changed
                -- anything (e.g. UpdatedAt); an UPDATE that only touched those is not recorded.
                CREATE OR REPLACE FUNCTION audit.log_changes() RETURNS trigger
                LANGUAGE plpgsql SECURITY DEFINER SET search_path = pg_catalog, public AS $fn$
                DECLARE
                    old_row JSONB;
                    new_row JSONB;
                    changed TEXT[] := '{}';
                    ignored TEXT[] := TG_ARGV;
                BEGIN
                    IF TG_OP IN ('UPDATE', 'DELETE') THEN old_row := to_jsonb(OLD); END IF;
                    IF TG_OP IN ('INSERT', 'UPDATE') THEN new_row := to_jsonb(NEW); END IF;

                    IF TG_OP = 'UPDATE' THEN
                        SELECT coalesce(array_agg(k ORDER BY k), '{}') INTO changed
                        FROM jsonb_object_keys(new_row) AS k
                        WHERE NOT (k = ANY (ignored)) AND (old_row -> k) IS DISTINCT FROM (new_row -> k);
                        IF cardinality(changed) = 0 THEN RETURN NULL; END IF;
                    END IF;

                    INSERT INTO audit.change_log
                        (table_name, operation, row_id, old_values, new_values, changed_fields, author_name, frontend_url, request_id)
                    VALUES (
                        TG_TABLE_NAME, TG_OP, coalesce(new_row ->> 'Id', old_row ->> 'Id'), old_row, new_row, changed,
                        nullif(current_setting('app.author_name', true), ''),
                        nullif(current_setting('app.frontend_url', true), ''),
                        nullif(current_setting('app.request_id', true), ''));
                    RETURN NULL;
                END
                $fn$;
                """);

            foreach (var table in Audited)
                migrationBuilder.Sql($"""
                    CREATE TRIGGER audit_changes AFTER INSERT OR UPDATE OR DELETE ON "{table}"
                    FOR EACH ROW EXECUTE FUNCTION audit.log_changes('UpdatedAt');
                    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Audited)
                migrationBuilder.Sql($"""DROP TRIGGER IF EXISTS audit_changes ON "{table}";""");
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS audit CASCADE;");
        }
    }
}
