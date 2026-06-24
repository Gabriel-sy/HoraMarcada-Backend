using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoraMarcada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProfessionalEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "professionals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "professionals",
                type: "text",
                nullable: true);
        }
    }
}
