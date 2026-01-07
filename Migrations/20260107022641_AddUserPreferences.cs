using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOfficeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultationId",
                table: "BillingInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RendezVousId",
                table: "BillingInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PreferenceKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PreferenceValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoices_ConsultationId",
                table: "BillingInvoices",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoices_RendezVousId",
                table: "BillingInvoices",
                column: "RendezVousId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingInvoices_Consultations_ConsultationId",
                table: "BillingInvoices",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingInvoices_RendezVous_RendezVousId",
                table: "BillingInvoices",
                column: "RendezVousId",
                principalTable: "RendezVous",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingInvoices_Consultations_ConsultationId",
                table: "BillingInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingInvoices_RendezVous_RendezVousId",
                table: "BillingInvoices");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_BillingInvoices_ConsultationId",
                table: "BillingInvoices");

            migrationBuilder.DropIndex(
                name: "IX_BillingInvoices_RendezVousId",
                table: "BillingInvoices");

            migrationBuilder.DropColumn(
                name: "ConsultationId",
                table: "BillingInvoices");

            migrationBuilder.DropColumn(
                name: "RendezVousId",
                table: "BillingInvoices");
        }
    }
}
