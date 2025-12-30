using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalOfficeManagement.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        [Required]
        public int MedecinId { get; set; }
        [ForeignKey("MedecinId")]
        public virtual Medecin Medecin { get; set; } = null!;

        // AJOUT : Lien avec la Salle (Affectation)
        [Required]
        public int SalleId { get; set; }
        [ForeignKey("SalleId")]
        public virtual Salle Salle { get; set; } = null!;

        [Required]
        public DayOfWeek JourSemaine { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan HeureArrivee { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan HeureDepart { get; set; }

        public string? Note { get; set; }
    }
}