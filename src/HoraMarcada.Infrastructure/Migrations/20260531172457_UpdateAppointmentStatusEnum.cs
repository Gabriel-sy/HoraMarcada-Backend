using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoraMarcada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_appointments_status",
                table: "appointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_appointments_status",
                table: "appointments",
                sql: "status IN ('confirmed', 'canceled', 'completed', 'confirmedbyuser')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_appointments_status",
                table: "appointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_appointments_status",
                table: "appointments",
                sql: "status IN ('confirmed', 'canceled', 'completed')");
        }
    }
}
