using MedicalAppointmentAPI.Controllers;
using MedicalAppointmentAPI.Models;
using MedicalAppointmentAPI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalAppointmentAPI.Tests.Controllers;

public class AppointmentsControllerTests
{
    [Fact]
    public async Task PostAppointment_ValidAppointment_ReturnsCreatedAtAction()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new AppointmentsController(context);

        var appointment = new Appointment
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDateTime = TestDbContextFactory.FutureMondayAt(10),
            DurationMinutes = 30,
            Status = "Scheduled"
        };

        var result = await controller.PostAppointment(appointment);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdAppointment = Assert.IsType<Appointment>(createdResult.Value);

        Assert.Equal(nameof(AppointmentsController.GetAppointment), createdResult.ActionName);
        Assert.Equal("Scheduled", createdAppointment.Status);
        Assert.Equal(1, await context.Appointments.CountAsync());
    }

    [Fact]
    public async Task PostAppointment_PastDate_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new AppointmentsController(context);

        var appointment = new Appointment
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDateTime = DateTimeOffset.Now.AddDays(-1),
            DurationMinutes = 30,
            Status = "Scheduled"
        };

        var result = await controller.PostAppointment(appointment);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostAppointment_ConflictingAppointment_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var firstAppointmentTime = TestDbContextFactory.FutureMondayAt(10);

        context.Appointments.Add(new Appointment
        {
            AppointmentId = 1,
            PatientId = 1,
            DoctorId = 1,
            AppointmentDateTime = firstAppointmentTime.ToUniversalTime(),
            DurationMinutes = 60,
            Status = "Scheduled"
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var controller = new AppointmentsController(context);

        var conflictingAppointment = new Appointment
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDateTime = firstAppointmentTime.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled"
        };

        var result = await controller.PostAppointment(conflictingAppointment);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAppointment_MissingId_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AppointmentsController(context);

        var result = await controller.GetAppointment(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAppointmentsByDoctor_MissingDoctor_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AppointmentsController(context);

        var result = await controller.GetAppointmentsByDoctor(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ChangeAppointmentStatus_ValidStatus_ReturnsNoContentAndUpdatesStatus()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        context.Appointments.Add(new Appointment
        {
            AppointmentId = 1,
            PatientId = 1,
            DoctorId = 1,
            AppointmentDateTime = TestDbContextFactory.FutureMondayAt(10).ToUniversalTime(),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var controller = new AppointmentsController(context);

        var result = await controller.ChangeAppointmentStatus(1, "Completed");

        Assert.IsType<NoContentResult>(result);

        var appointmentFromDb = await context.Appointments.FindAsync(1);
        Assert.NotNull(appointmentFromDb);
        Assert.Equal("Completed", appointmentFromDb.Status);
    }

    [Fact]
    public async Task ChangeAppointmentStatus_EmptyStatus_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AppointmentsController(context);

        var result = await controller.ChangeAppointmentStatus(1, "");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAppointment_MissingAppointment_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AppointmentsController(context);

        var result = await controller.DeleteAppointment(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}