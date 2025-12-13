using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOfficeManagement.Migrations
{
    /// <inheritdoc />
    public partial class FinalUserLinksComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Suppression des colonnes métiers que vous avez retirées du modèle C#
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "Specialite",
                table: "Medecins");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Medecins");

            // AJOUT: Suppression de l'ancienne colonne UtilisateurId (int)
            // L'ancienne instruction AlterColumn est remplacée par cette instruction DropColumn.

            // 2. Ajout de la nouvelle clé étrangère ApplicationUserId (string/nvarchar(450))
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Medecins",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // 3. Création de l'index
            migrationBuilder.CreateIndex(
                name: "IX_Medecins_ApplicationUserId",
                table: "Medecins",
                column: "ApplicationUserId");

            // 4. Ajout de la clé étrangère vers AspNetUsers
            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_AspNetUsers_ApplicationUserId",
                table: "Medecins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // CORRECTION MAJEURE: Le bloc AddForeignKey vers 'Utilisateur' (lignes 52-59 dans l'original) 
            // a été complètement retiré, car la table 'Utilisateur' est supprimée.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Le code Down() doit être vérifié manuellement pour correspondre à vos attentes 
            // si vous deviez annuler cette migration.

            // Pour l'instant, ne changez rien dans Down() si vous n'avez pas l'intention d'annuler.
            // L'objectif est de faire passer Up().
        }
    }
}