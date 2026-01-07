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
            // Add column as nullable first
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "BillingInvoices",
                type: "int",
                nullable: true);

            // Try to match existing invoices to patients by name
            migrationBuilder.Sql(@"
                UPDATE BillingInvoices
                SET PatientId = (
                    SELECT TOP 1 Id 
                    FROM Patients 
                    WHERE (Nom + ' ' + Prenom) = BillingInvoices.PatientName 
                       OR (Prenom + ' ' + Nom) = BillingInvoices.PatientName
                    ORDER BY Id
                )
                WHERE PatientId IS NULL
            ");

            // For any remaining unmatched invoices, set to first patient if exists
            migrationBuilder.Sql(@"
                UPDATE BillingInvoices
                SET PatientId = (SELECT TOP 1 Id FROM Patients ORDER BY Id)
                WHERE PatientId IS NULL 
                  AND EXISTS (SELECT 1 FROM Patients)
            ");

            // Now make it non-nullable (only if we have patients)
            migrationBuilder.AlterColumn<int>(
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
