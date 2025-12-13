using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class Ordonnance
{
    public int Id { get; set; }

    public int ConsultationId { get; set; }

    public string Contenu { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public virtual Consultation Consultation { get; set; } = null!;
}
