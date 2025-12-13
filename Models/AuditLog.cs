using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class AuditLog
{
    public long Id { get; set; }

    public int? UtilisateurId { get; set; }

    public string Action { get; set; } = null!;

    public string TableCible { get; set; } = null!;

    public int? EnregistrementId { get; set; }

    public DateTime Horodatage { get; set; }

    public string? Details { get; set; }

    public virtual Utilisateur? Utilisateur { get; set; }
}
