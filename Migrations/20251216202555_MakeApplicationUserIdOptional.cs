using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOfficeManagement.Migrations
{
    /// <inheritdoc />
    public partial class MakeApplicationUserIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🛑 MODIFICATION 1 : COMMENTER la suppression des FK basées sur l'ancienne table Utilisateur
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_Medecins_Utilisateur_UtilisateurId",
                table: "Medecins");
            */

            // Note : Cette ligne DOIT rester pour la colonne Medecin:ApplicationUserId
            migrationBuilder.DropForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins");


            // 🛑 MODIFICATION 2 : COMMENTER toutes les manipulations de l'ancienne table Utilisateur
            /*
            migrationBuilder.DropTable(
                name: "Utilisateur");

            migrationBuilder.DropIndex(
                name: "IX_Medecins_UtilisateurId",
                table: "Medecins");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_UtilisateurId",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "UtilisateurId",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "UtilisateurId",
                table: "audit_log");
            */

            // Ceci est la seule ligne essentielle pour le scénario actuel (rendre l'ID nullable)
            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Medecins",
                type: "nvarchar(450)",
                nullable: true, // <--- C'est ici que l'on passe à nullable
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Ajout des nouvelles colonnes Prenom/Specialite
            migrationBuilder.AddColumn<string>(
                name: "Prenom",
                table: "Medecins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specialite",
                table: "Medecins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Ajout de la nouvelle colonne ApplicationUserId dans audit_log (elle est nullable, c'est bien)
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "audit_log",
                type: "nvarchar(450)",
                nullable: true);

            // Mises à jour des tables Identity standard pour EF Core 
            // (Ces lignes sont généralement sécuritaires, nous les conservons)
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            // Création de l'index pour la nouvelle FK ApplicationUserId sur audit_log
            migrationBuilder.CreateIndex(
                name: "IX_audit_log_ApplicationUserId",
                table: "audit_log",
                column: "ApplicationUserId");

            // Rétablissement des clés étrangères basées sur AspNetUsers
            migrationBuilder.AddForeignKey(
                name: "FK_audit_log_AspNetUsers_ApplicationUserId",
                table: "audit_log",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id"); // Note : principalColumn est Id par défaut

            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id"); // Note : principalColumn est Id par défaut
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // La méthode Down tente de revenir à l'état antérieur (avec l'ancienne table Utilisateur).
            // Nous la laissons intacte, car elle est nécessaire pour annuler la migration.
            // Cependant, si Down échoue plus tard, nous devrons la nettoyer aussi.

            migrationBuilder.DropForeignKey(
                name: "FK_audit_log_AspNetUsers_ApplicationUserId",
                table: "audit_log");

            migrationBuilder.DropForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_ApplicationUserId",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "Specialite",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "audit_log");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "Medecins",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UtilisateurId",
                table: "Medecins",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UtilisateurId",
                table: "audit_log",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "Utilisateur",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotDePasse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateur", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medecins_UtilisateurId",
                table: "Medecins",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_UtilisateurId",
                table: "audit_log",
                column: "UtilisateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_audit_log_Utilisateur_UtilisateurId",
                table: "audit_log",
                column: "UtilisateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_Utilisateur_UtilisateurId",
                table: "Medecins",
                column: "UtilisateurId",
                principalTable: "Utilisateur",
                principalColumn: "Id");
        }
    }
}