using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Linq;
using FluentAssertions;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedicalOfficeManagement.IntegrationTests;

public class PatientPortalControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PatientPortalControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AppointmentStatus_ReturnsNotFound_ForAppointmentOwnedByAnotherPatient()
    {
        var patientOne = CreatePatient("p1");
        var patientTwo = CreatePatient("p2");
        var doctor = CreateMedecin(1);
        var appointmentForPatientTwo = new RendezVou
        {
            Patient = patientTwo,
            Medecin = doctor,
            DateDebut = DateTime.UtcNow.AddDays(2),
            DateFin = DateTime.UtcNow.AddDays(2).AddHours(1),
            Statut = "Confirmed",
            Motif = "Follow up"
        };

        await SeedDataAsync(context =>
        {
            context.Patients.AddRange(patientOne, patientTwo);
            context.Medecins.Add(doctor);
            context.RendezVous.Add(appointmentForPatientTwo);
        });

        var client = CreateAuthenticatedClient(patientOne.ApplicationUserId!);

        var response = await client.GetAsync($"/PatientPortal/AppointmentStatus/{appointmentForPatientTwo.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AppointmentStatus_ReturnsAppointment_ForOwner()
    {
        var patient = CreatePatient("owner-1");
        var doctor = CreateMedecin(2);
        var appointment = new RendezVou
        {
            Patient = patient,
            Medecin = doctor,
            DateDebut = DateTime.UtcNow.AddDays(3),
            DateFin = DateTime.UtcNow.AddDays(3).AddHours(1),
            Statut = "Pending Approval",
            Motif = "Consultation"
        };

        await SeedDataAsync(context =>
        {
            context.Patients.Add(patient);
            context.Medecins.Add(doctor);
            context.RendezVous.Add(appointment);
        });

        var client = CreateAuthenticatedClient(patient.ApplicationUserId!);

        var response = await client.GetAsync($"/PatientPortal/AppointmentStatus/{appointment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<PatientAppointmentViewModel>();
        payload.Should().NotBeNull();
        payload!.Id.Should().Be(appointment.Id);
        payload.Status.Should().Be("Pending Approval");
        payload.Reason.Should().Be("Consultation");
    }

    [Fact]
    public async Task RequestRefill_AllowsOwnerAndUpdatesPrescription()
    {
        var patient = CreatePatient("refill-owner");
        var prescription = new Prescription
        {
            Patient = patient,
            Medication = "Medication A",
            Dosage = "10mg",
            Frequency = "Once daily",
            Status = "Active",
            RefillsRemaining = 2,
            IssuedOn = DateTime.UtcNow.AddDays(-10)
        };

        await SeedDataAsync(context =>
        {
            context.Patients.Add(patient);
            context.Prescriptions.Add(prescription);
        });

        var client = CreateAuthenticatedClient(patient.ApplicationUserId!);
        var response = await client.PostAsync($"/PatientPortal/RequestRefill?prescriptionId={prescription.Id}", new FormUrlEncodedContent([]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        payload.Should().NotBeNull();
        payload!["message"].Should().Contain("renouvellement");

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalOfficeContext>();
        var updated = await context.Prescriptions.FindAsync(prescription.Id);
        updated!.Status.Should().Be("Refill Requested");
        updated.NextRefill.Should().NotBeNull();
    }

    [Fact]
    public async Task RequestRefill_ReturnsNotFound_ForNonOwner()
    {
        var patientOne = CreatePatient("refill-owner-1");
        var patientTwo = CreatePatient("refill-owner-2");
        var prescription = new Prescription
        {
            Patient = patientTwo,
            Medication = "Medication B",
            Status = "Active",
            RefillsRemaining = 1,
            IssuedOn = DateTime.UtcNow.AddDays(-5)
        };

        await SeedDataAsync(context =>
        {
            context.Patients.AddRange(patientOne, patientTwo);
            context.Prescriptions.Add(prescription);
        });

        var client = CreateAuthenticatedClient(patientOne.ApplicationUserId!);
        var response = await client.PostAsync($"/PatientPortal/RequestRefill?prescriptionId={prescription.Id}", new FormUrlEncodedContent([]));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LabNotifications_ReturnsResultsForCurrentPatientOnly()
    {
        var patientOne = CreatePatient("lab-owner-1");
        var patientTwo = CreatePatient("lab-owner-2");
        var doctor = CreateMedecin(33);

        var patientOneResult = new LabResult
        {
            Patient = patientOne,
            Medecin = doctor,
            TestName = "Blood Panel",
            Status = "Available",
            CollectedOn = DateTime.UtcNow.AddDays(-1),
            ResultValue = "Normal"
        };

        var patientTwoResult = new LabResult
        {
            Patient = patientTwo,
            Medecin = doctor,
            TestName = "MRI",
            Status = "Pending",
            CollectedOn = DateTime.UtcNow,
            ResultValue = "Review"
        };

        await SeedDataAsync(context =>
        {
            context.Patients.AddRange(patientOne, patientTwo);
            context.Medecins.Add(doctor);
            context.LabResults.AddRange(patientOneResult, patientTwoResult);
        });

        var client = CreateAuthenticatedClient(patientOne.ApplicationUserId!);
        var response = await client.GetAsync("/PatientPortal/LabNotifications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var results = await response.Content.ReadFromJsonAsync<List<PatientResultViewModel>>();
        results.Should().NotBeNull();
        results!.Should().HaveCount(1);
        results.First().Title.Should().Be("Blood Panel");
        results.First().Id.Should().Be(patientOneResult.Id);
    }

    private HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, userId);
        return client;
    }

    private static Patient CreatePatient(string userId) => new()
    {
        ApplicationUserId = userId,
        Nom = "Doe",
        Prenom = "John",
        Telephone = "0000000000",
        Email = $"{userId}@example.com",
        Adresse = "123 rue de test"
    };

    private static Medecin CreateMedecin(int id) => new()
    {
        Id = id,
        NomPrenom = "Dr Tester",
        Specialite = "General",
        Adresse = "123 street",
        Telephone = "1111111111",
        Email = $"doctor{id}@example.com",
        ApplicationUserId = $"doctor-user-{id}"
    };

    private async Task SeedDataAsync(Action<MedicalOfficeContext> seeder)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalOfficeContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        seeder(context);
        await context.SaveChangesAsync();
    }
}
