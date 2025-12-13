using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Ajout nécessaire pour l'attribut [ForeignKey]

namespace MedicalOfficeManagement.Models;

public partial class AuditLog
{
    public long Id { get; set; }

    // MODIFICATION 1 : Remplacement de l'ancienne clé int? par la nouvelle clé string d'Identity
    // Identity utilise un string (nvarchar(450)) comme clé primaire.
    public string? ApplicationUserId { get; set; } // Nouvelle clé étrangère

    // L'ancienne propriété UtilisateurId (int?) est supprimée ou renommée pour éviter les conflits.

    public string Action { get; set; } = null!;

    public string TableCible { get; set; } = null!;

    public int? EnregistrementId { get; set; }

    public DateTime Horodatage { get; set; }

    public string? Details { get; set; }

    // MODIFICATION 2 : Mise à jour de la propriété de navigation
    // Elle doit pointer vers votre classe utilisateur Identity (ApplicationUser)
    [ForeignKey("ApplicationUserId")]
    public virtual ApplicationUser? ApplicationUser { get; set; }
}