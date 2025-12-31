using MedicalOfficeManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(MedicalOfficeDbContext context, CancellationToken cancellationToken = default)
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);

            if (await context.Doctors.AnyAsync(cancellationToken))
            {
                return;
            }

            var doctors = new[]
            {
                new DoctorEntity { Id = "DOC-100", FullName = "Dr. Amelia Rodriguez", Specialty = "Cardiology", ClinicId = "CLINIC-A", Phone = "555-0100", Email = "amelia.rodriguez@medical.test", Location = "Heart Center" },
                new DoctorEntity { Id = "DOC-200", FullName = "Dr. Marcus Lee", Specialty = "Family Medicine", ClinicId = "CLINIC-A", Phone = "555-0101", Email = "marcus.lee@medical.test", Location = "Primary Care" },
                new DoctorEntity { Id = "DOC-300", FullName = "Dr. Priya Nair", Specialty = "Pediatrics", ClinicId = "CLINIC-B", Phone = "555-0102", Email = "priya.nair@medical.test", Location = "Pediatric Wing" },
                new DoctorEntity { Id = "DOC-400", FullName = "Dr. Ethan Chen", Specialty = "Orthopedics", ClinicId = "CLINIC-B", Phone = "555-0103", Email = "ethan.chen@medical.test", Location = "Ortho Clinic" }
            };

            var patients = new[]
            {
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Sarah", LastName = "Johnson", MedicalRecordNumber = "MRN-1001", DateOfBirth = DateTime.Today.AddYears(-32), RiskLevel = "Chronic", PrimaryPhysicianId = "DOC-200", Phone = "555-1001", Email = "sarah.johnson@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Michael", LastName = "Chen", MedicalRecordNumber = "MRN-1002", DateOfBirth = DateTime.Today.AddYears(-45), RiskLevel = "High", PrimaryPhysicianId = "DOC-100", Phone = "555-1002", Email = "michael.chen@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Emma", LastName = "Williams", MedicalRecordNumber = "MRN-1003", DateOfBirth = DateTime.Today.AddYears(-29), RiskLevel = "Low", PrimaryPhysicianId = "DOC-300", Phone = "555-1003", Email = "emma.williams@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "James", LastName = "Martinez", MedicalRecordNumber = "MRN-1004", DateOfBirth = DateTime.Today.AddYears(-52), RiskLevel = "Moderate", PrimaryPhysicianId = "DOC-200", Phone = "555-1004", Email = "james.martinez@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Olivia", LastName = "Brown", MedicalRecordNumber = "MRN-1005", DateOfBirth = DateTime.Today.AddYears(-24), RiskLevel = "Low", PrimaryPhysicianId = "DOC-300", Phone = "555-1005", Email = "olivia.brown@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Liam", LastName = "Garcia", MedicalRecordNumber = "MRN-1006", DateOfBirth = DateTime.Today.AddYears(-38), RiskLevel = "High", PrimaryPhysicianId = "DOC-100", Phone = "555-1006", Email = "liam.garcia@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Ava", LastName = "Davis", MedicalRecordNumber = "MRN-1007", DateOfBirth = DateTime.Today.AddYears(-41), RiskLevel = "Moderate", PrimaryPhysicianId = "DOC-400", Phone = "555-1007", Email = "ava.davis@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Noah", LastName = "Wilson", MedicalRecordNumber = "MRN-1008", DateOfBirth = DateTime.Today.AddYears(-36), RiskLevel = "Low", PrimaryPhysicianId = "DOC-200", Phone = "555-1008", Email = "noah.wilson@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Isabella", LastName = "Lopez", MedicalRecordNumber = "MRN-1009", DateOfBirth = DateTime.Today.AddYears(-33), RiskLevel = "Chronic", PrimaryPhysicianId = "DOC-300", Phone = "555-1009", Email = "isabella.lopez@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "William", LastName = "Anderson", MedicalRecordNumber = "MRN-1010", DateOfBirth = DateTime.Today.AddYears(-47), RiskLevel = "Moderate", PrimaryPhysicianId = "DOC-400", Phone = "555-1010", Email = "william.anderson@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Mia", LastName = "Thomas", MedicalRecordNumber = "MRN-1011", DateOfBirth = DateTime.Today.AddYears(-27), RiskLevel = "Low", PrimaryPhysicianId = "DOC-300", Phone = "555-1011", Email = "mia.thomas@example.com" },
                new PatientEntity { Id = Guid.NewGuid(), FirstName = "Benjamin", LastName = "Harris", MedicalRecordNumber = "MRN-1012", DateOfBirth = DateTime.Today.AddYears(-55), RiskLevel = "High", PrimaryPhysicianId = "DOC-100", Phone = "555-1012", Email = "benjamin.harris@example.com" }
            };

            var today = DateTime.Today;
            var appointments = new List<AppointmentEntity>();

            void AddAppointment(int daysFromToday, int startHour, int durationMinutes, Guid patientId, string doctorId, AppointmentStatus status, string room)
            {
                var start = today.AddDays(daysFromToday).AddHours(startHour);
                appointments.Add(new AppointmentEntity
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    DoctorId = doctorId,
                    StartTime = start,
                    EndTime = start.AddMinutes(durationMinutes),
                    Status = status,
                    ClinicId = "CLINIC-A",
                    Room = room
                });
            }

            AddAppointment(-6, 9, 30, patients[0].Id, "DOC-200", AppointmentStatus.Completed, "Room 1");
            AddAppointment(-3, 10, 45, patients[1].Id, "DOC-100", AppointmentStatus.Completed, "Cardio 2");
            AddAppointment(-1, 13, 30, patients[2].Id, "DOC-300", AppointmentStatus.Completed, "Peds 1");
            AddAppointment(0, 9, 30, patients[3].Id, "DOC-200", AppointmentStatus.Confirmed, "Room 2");
            AddAppointment(0, 10, 30, patients[4].Id, "DOC-300", AppointmentStatus.Waiting, "Peds 1");
            AddAppointment(0, 11, 45, patients[5].Id, "DOC-100", AppointmentStatus.Waiting, "Cardio 1");
            AddAppointment(1, 9, 30, patients[6].Id, "DOC-400", AppointmentStatus.Confirmed, "Ortho 3");
            AddAppointment(1, 10, 30, patients[7].Id, "DOC-200", AppointmentStatus.Confirmed, "Room 3");
            AddAppointment(2, 14, 30, patients[8].Id, "DOC-300", AppointmentStatus.Confirmed, "Peds 2");
            AddAppointment(3, 15, 60, patients[9].Id, "DOC-400", AppointmentStatus.Waiting, "Ortho 1");
            AddAppointment(5, 9, 30, patients[10].Id, "DOC-300", AppointmentStatus.Confirmed, "Peds 1");
            AddAppointment(7, 10, 30, patients[11].Id, "DOC-100", AppointmentStatus.Waiting, "Cardio 1");

            context.Doctors.AddRange(doctors);
            context.Patients.AddRange(patients);
            context.Appointments.AddRange(appointments);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
