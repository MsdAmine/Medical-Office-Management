using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOfficeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIdToBillingInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "BillingInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoices_PatientId",
                table: "BillingInvoices",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingInvoices_Patients_PatientId",
                table: "BillingInvoices",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingInvoices_Patients_PatientId",
                table: "BillingInvoices");

            migrationBuilder.DropIndex(
                name: "IX_BillingInvoices_PatientId",
                table: "BillingInvoices");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "BillingInvoices");
        }
    }
}
