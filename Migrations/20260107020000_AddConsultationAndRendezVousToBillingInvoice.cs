using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOfficeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationAndRendezVousToBillingInvoice : Migration
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

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoices_ConsultationId",
                table: "BillingInvoices",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoices_RendezVousId",
                table: "BillingInvoices",
                column: "RendezVousId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingInvoices_Consultations_ConsultationId",
                table: "BillingInvoices",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingInvoices_RendezVous_RendezVousId",
                table: "BillingInvoices",
                column: "RendezVousId",
                principalTable: "RendezVous",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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
