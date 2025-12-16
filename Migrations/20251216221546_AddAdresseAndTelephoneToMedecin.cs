using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOfficeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddAdresseAndTelephoneToMedecin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins");

            migrationBuilder.RenameColumn(
                name: "Prenom",
                table: "Medecins",
                newName: "Telephone");

            migrationBuilder.RenameColumn(
                name: "Nom",
                table: "Medecins",
                newName: "Email");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Medecins",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Adresse",
                table: "Medecins",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomPrenom",
                table: "Medecins",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "Adresse",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "NomPrenom",
                table: "Medecins");

            migrationBuilder.RenameColumn(
                name: "Telephone",
                table: "Medecins",
                newName: "Prenom");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Medecins",
                newName: "Nom");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Medecins",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
