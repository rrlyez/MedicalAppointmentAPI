using MedicalAppointmentAPI.Controllers;
using MedicalAppointmentAPI.Models;
using MedicalAppointmentAPI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalAppointmentAPI.Tests.Controllers;

public class PatientsControllerTests
{
    [Fact]
    public async Task GetPatients_ReturnsPatientsOrderedById()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Patients.AddRange(
            new Patient
            {
                PatientId = 2,
                FullName = "Second Patient",
                Phone = "+380501111111",
                Email = "second@example.com"
            },
            new Patient
            {
                PatientId = 1,
                FullName = "First Patient",
                Phone = "+380502222222",
                Email = "first@example.com"
            }
        );

        await context.SaveChangesAsync();

        var controller = new PatientsController(context);

        var result = await controller.GetPatients();

        var patients = Assert.IsAssignableFrom<IEnumerable<Patient>>(result.Value);
        Assert.Equal([1, 2], patients.Select(p => p.PatientId).ToList());
    }

    [Fact]
    public async Task GetPatient_ExistingId_ReturnsPatient()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Patients.Add(new Patient
        {
            PatientId = 1,
            FullName = "Anna Brown",
            Phone = "+380501112233",
            Email = "anna@example.com"
        });

        await context.SaveChangesAsync();

        var controller = new PatientsController(context);

        var result = await controller.GetPatient(1);

        var patient = Assert.IsType<Patient>(result.Value);
        Assert.Equal("Anna Brown", patient.FullName);
    }

    [Fact]
    public async Task GetPatient_MissingId_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new PatientsController(context);

        var result = await controller.GetPatient(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPatient_DuplicateEmail_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Patients.Add(new Patient
        {
            PatientId = 1,
            FullName = "Anna Brown",
            Phone = "+380501112233",
            Email = "anna@example.com"
        });

        await context.SaveChangesAsync();

        var controller = new PatientsController(context);

        var newPatient = new Patient
        {
            FullName = "Maria White",
            Phone = "+380507778899",
            Email = "anna@example.com"
        };

        var result = await controller.PostPatient(newPatient);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPatient_ValidPatient_ReturnsCreatedAtAction()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new PatientsController(context);

        var patient = new Patient
        {
            FullName = "Maria White",
            Phone = "+380507778899",
            Email = "maria@example.com"
        };

        var result = await controller.PostPatient(patient);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdPatient = Assert.IsType<Patient>(createdResult.Value);

        Assert.Equal(nameof(PatientsController.GetPatient), createdResult.ActionName);
        Assert.Equal("Maria White", createdPatient.FullName);
        Assert.Equal(1, await context.Patients.CountAsync());
    }

    [Fact]
    public async Task SearchPatients_EmptyName_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new PatientsController(context);

        var result = await controller.SearchPatients("");

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}