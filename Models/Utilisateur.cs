using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class Utilisateur
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MotDePasse { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Medecin> Medecins { get; set; } = new List<Medecin>();
}
