using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoraMarcada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_categories_store",
                table: "categories",
                column: "store_id");

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX idx_categories_store_name_lower
                ON categories (store_id, lower(name));
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "services",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                WITH stores_with_services AS (
                    SELECT DISTINCT store_id FROM services
                ),
                inserted AS (
                    INSERT INTO categories (id, store_id, name)
                    SELECT gen_random_uuid(), store_id, 'Geral'
                    FROM stores_with_services
                    RETURNING id, store_id
                )
                UPDATE services s
                SET category_id = i.id
                FROM inserted i
                WHERE s.store_id = i.store_id;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "category_id",
                table: "services",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_category_id",
                table: "services",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_services_categories_category_id",
                table: "services",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_services_categories_category_id",
                table: "services");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropIndex(
                name: "IX_services_category_id",
                table: "services");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "services");
        }
    }
}
