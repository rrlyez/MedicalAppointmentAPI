using MedicalAppointmentAPI.Controllers;
using MedicalAppointmentAPI.Models;
using MedicalAppointmentAPI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalAppointmentAPI.Tests.Controllers;

public class DoctorsControllerTests
{
    [Fact]
    public async Task GetDoctor_ExistingId_ReturnsDoctor()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new DoctorsController(context);

        var result = await controller.GetDoctor(1);

        var doctor = Assert.IsType<Doctor>(result.Value);
        Assert.Equal("John Smith", doctor.FullName);
    }

    [Fact]
    public async Task GetDoctor_MissingId_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new DoctorsController(context);

        var result = await controller.GetDoctor(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostDoctor_ValidDoctor_ReturnsCreatedAtAction()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Specializations.Add(new Specialization
        {
            SpecializationId = 1,
            Name = "Cardiology"
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var controller = new DoctorsController(context);

        var doctor = new Doctor
        {
            FullName = "Emily Stone",
            SpecializationId = 1
        };

        var result = await controller.PostDoctor(doctor);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdDoctor = Assert.IsType<Doctor>(createdResult.Value);

        Assert.Equal(nameof(DoctorsController.GetDoctor), createdResult.ActionName);
        Assert.Equal("Emily Stone", createdDoctor.FullName);
        Assert.Equal(1, await context.Doctors.CountAsync());
    }

    [Fact]
    public async Task PostDoctor_MissingSpecialization_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new DoctorsController(context);

        var doctor = new Doctor
        {
            FullName = "Emily Stone",
            SpecializationId = 999
        };

        var result = await controller.PostDoctor(doctor);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteDoctor_WithSchedule_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new DoctorsController(context);

        var result = await controller.DeleteDoctor(1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetDoctorsBySpecialization_ReturnsFilteredDoctors()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        context.Specializations.Add(new Specialization
        {
            SpecializationId = 2,
            Name = "Neurology"
        });

        context.Doctors.Add(new Doctor
        {
            DoctorId = 2,
            FullName = "Another Doctor",
            SpecializationId = 2
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var controller = new DoctorsController(context);

        var result = await controller.GetDoctorsBySpecialization(1);

        var doctors = Assert.IsAssignableFrom<IEnumerable<Doctor>>(result.Value);
        Assert.Single(doctors);
        Assert.Equal("John Smith", doctors.First().FullName);
    }
}