using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoraMarcada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "customers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "customers",
                type: "text",
                nullable: true);
        }
    }
}
