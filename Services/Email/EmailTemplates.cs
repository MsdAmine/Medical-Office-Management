using MedicalOfficeManagement.Models;

namespace MedicalOfficeManagement.Services.Email
{
    public static class EmailTemplates
    {
        public static string AppointmentApproved(RendezVou appointment)
        {
            return $"""
            <h2>Appointment Approved</h2>
            <p>
                Patient: {appointment.Patient?.Prenom} {appointment.Patient?.Nom}<br/>
                Date: {appointment.DateDebut:dddd, MMM dd yyyy}<br/>
                Time: {appointment.DateDebut:HH:mm}
            </p>
            <p>Medical Office</p>
            """;
        }

        public static string AppointmentCancelled(RendezVou appointment)
        {
            return $"""
            <h2>Appointment Cancelled</h2>
            <p>
                Date: {appointment.DateDebut:dddd, MMM dd yyyy}<br/>
                Reason: {appointment.Motif ?? "Not specified"}
            </p>
            """;
        }
    }
}
