using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoraMarcada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAppointmentsAndCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS customers (
                    id UUID PRIMARY KEY,
                    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE CASCADE,
                    name TEXT NOT NULL,
                    whatsapp TEXT NOT NULL,
                    email TEXT,
                    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                    CONSTRAINT idx_customers_store_whatsapp UNIQUE (store_id, whatsapp)
                );

                CREATE INDEX IF NOT EXISTS idx_customers_store ON customers(store_id);

                INSERT INTO customers (id, store_id, name, whatsapp, email, created_at)
                SELECT 
                    gen_random_uuid() AS id,
                    store_id,
                    customer_name,
                    customer_whatsapp,
                    customer_email,
                    created_at
                FROM (
                    SELECT 
                        store_id,
                        customer_name,
                        customer_whatsapp,
                        customer_email,
                        created_at,
                        ROW_NUMBER() OVER (
                            PARTITION BY store_id, customer_whatsapp 
                            ORDER BY created_at DESC
                        ) AS rn
                    FROM appointments
                ) t
                WHERE rn = 1
                ON CONFLICT (store_id, whatsapp) DO NOTHING;

                UPDATE appointments a
                SET customer_id = c.id
                FROM customers c
                WHERE a.store_id = c.store_id 
                  AND a.customer_whatsapp = c.whatsapp
                  AND a.customer_id IS NULL;

                DO $$
                DECLARE
                    orphan_record RECORD;
                    new_cust_id UUID;
                BEGIN
                    FOR orphan_record IN 
                        SELECT DISTINCT store_id, customer_name, customer_whatsapp, customer_email 
                        FROM appointments 
                        WHERE customer_id IS NULL
                    LOOP
                        new_cust_id := gen_random_uuid();
                        INSERT INTO customers (id, store_id, name, whatsapp, email, created_at)
                        VALUES (new_cust_id, orphan_record.store_id, orphan_record.customer_name, orphan_record.customer_whatsapp, orphan_record.customer_email, NOW())
                        ON CONFLICT (store_id, whatsapp) DO UPDATE SET id = customers.id
                        RETURNING id INTO new_cust_id;

                        UPDATE appointments 
                        SET customer_id = new_cust_id 
                        WHERE store_id = orphan_record.store_id 
                          AND customer_whatsapp = orphan_record.customer_whatsapp 
                          AND customer_id IS NULL;
                    END LOOP;
                END $$;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "customer_email",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "customer_name",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "customer_whatsapp",
                table: "appointments");

            migrationBuilder.AlterColumn<Guid>(
                name: "customer_id",
                table: "appointments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments");

            migrationBuilder.AlterColumn<Guid>(
                name: "customer_id",
                table: "appointments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "customer_email",
                table: "appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_whatsapp",
                table: "appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_customers_customer_id",
                table: "appointments",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
