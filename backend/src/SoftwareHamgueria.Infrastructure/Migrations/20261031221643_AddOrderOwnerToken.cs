using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftwareHamgueria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderOwnerToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerTokenHash",
                table: "Orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerTokenHash",
                table: "Orders");
        }
    }
}
