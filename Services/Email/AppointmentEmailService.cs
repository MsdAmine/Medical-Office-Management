using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Services.Email;

namespace MedicalOfficeManagement.Services.Email
{
    public class AppointmentEmailService
    {
        private readonly IEmailSender _emailSender;

        public AppointmentEmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task SendConfirmationAsync(RendezVou rdv)
        {
            if (rdv.Patient == null || string.IsNullOrWhiteSpace(rdv.Patient.Email))
                return;

            var subject = "Appointment Confirmation";

            var body = $@"
                <h2>Appointment Confirmed</h2>
                <p>Dear {rdv.Patient.Nom},</p>

                <p>Your appointment has been successfully confirmed.</p>

                <ul>
                    <li><strong>Date:</strong> {rdv.DateDebut:dddd, dd MMMM yyyy}</li>
                    <li><strong>Time:</strong> {rdv.DateDebut:HH:mm}</li>
                    <li><strong>Doctor:</strong> Dr. {rdv.Medecin?.NomPrenom}</li>
                </ul>

                <p>Please arrive 10 minutes early.</p>

                <p><strong>Medical Office</strong></p>
            ";

            await _emailSender.SendAsync(
                rdv.Patient.Email,
                subject,
                body
            );
        }
    }
}
