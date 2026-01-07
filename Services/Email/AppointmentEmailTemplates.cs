using MedicalOfficeManagement.Models;

namespace MedicalOfficeManagement.Services.Email
{
    public static class AppointmentEmailTemplates
    {
        public static string AppointmentApproved(RendezVou appointment)
        {
            return $@"
                <h2>Appointment Confirmed</h2>

                <p>Dear {appointment.Patient?.Prenom} {appointment.Patient?.Nom},</p>

                <p>Your appointment has been approved and scheduled.</p>

                <ul>
                    <li><strong>Date:</strong> {appointment.DateDebut:dddd, dd MMMM yyyy}</li>
                    <li><strong>Time:</strong> {appointment.DateDebut:HH:mm}</li>
                    <li><strong>Doctor:</strong> {appointment.Medecin?.NomPrenom}</li>
                </ul>

                <p>Please arrive 10 minutes early.</p>

                <p><strong>Medical Office</strong></p>
            ";
        }
    }
}
