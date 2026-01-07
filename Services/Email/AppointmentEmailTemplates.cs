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

        public static string AppointmentReminder(RendezVou appointment)
        {
            var roomInfo = appointment.SalleId.HasValue 
                ? $"Room {appointment.SalleId}" 
                : "Room will be assigned";

            return $@"
                <h2>Appointment Reminder</h2>

                <p>Dear {appointment.Patient?.Prenom} {appointment.Patient?.Nom},</p>

                <p>This is a reminder that you have an upcoming appointment.</p>

                <ul>
                    <li><strong>Date:</strong> {appointment.DateDebut:dddd, dd MMMM yyyy}</li>
                    <li><strong>Time:</strong> {appointment.DateDebut:HH:mm} - {appointment.DateFin:HH:mm}</li>
                    <li><strong>Doctor:</strong> {appointment.Medecin?.NomPrenom}</li>
                    <li><strong>Room:</strong> {roomInfo}</li>
                    {(string.IsNullOrWhiteSpace(appointment.Motif) ? "" : $"<li><strong>Reason:</strong> {appointment.Motif}</li>")}
                </ul>

                <p>Please arrive 10 minutes early for check-in.</p>

                <p>If you need to reschedule or cancel, please contact us as soon as possible.</p>

                <p><strong>Medical Office</strong></p>
            ";
        }
    }
}

