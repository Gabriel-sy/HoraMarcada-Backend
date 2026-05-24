using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoraMarcada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "stores",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "professionals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_email",
                table: "appointments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "email",
                table: "professionals");

            migrationBuilder.DropColumn(
                name: "customer_email",
                table: "appointments");
        }
    }
}
