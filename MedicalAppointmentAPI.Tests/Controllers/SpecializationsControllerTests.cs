using MedicalAppointmentAPI.Controllers;
using MedicalAppointmentAPI.Models;
using MedicalAppointmentAPI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalAppointmentAPI.Tests.Controllers;

public class SpecializationsControllerTests
{
    [Fact]
    public async Task GetSpecializations_ReturnsSpecializationsOrderedById()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Specializations.AddRange(
            new Specialization
            {
                SpecializationId = 2,
                Name = "Neurology"
            },
            new Specialization
            {
                SpecializationId = 1,
                Name = "Cardiology"
            }
        );

        await context.SaveChangesAsync();

        var controller = new SpecializationsController(context);

        var result = await controller.GetSpecializations();

        var specializations = Assert.IsAssignableFrom<IEnumerable<Specialization>>(result.Value);
        Assert.Equal([1, 2], specializations.Select(s => s.SpecializationId).ToList());
    }

    [Fact]
    public async Task PostSpecialization_ValidSpecialization_ReturnsCreatedAtAction()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new SpecializationsController(context);

        var specialization = new Specialization
        {
            Name = "Dermatology"
        };

        var result = await controller.PostSpecialization(specialization);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdSpecialization = Assert.IsType<Specialization>(createdResult.Value);

        Assert.Equal(nameof(SpecializationsController.GetSpecialization), createdResult.ActionName);
        Assert.Equal("Dermatology", createdSpecialization.Name);
        Assert.Equal(1, await context.Specializations.CountAsync());
    }

    [Fact]
    public async Task PostSpecialization_DuplicateName_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Specializations.Add(new Specialization
        {
            SpecializationId = 1,
            Name = "Cardiology"
        });

        await context.SaveChangesAsync();

        var controller = new SpecializationsController(context);

        var specialization = new Specialization
        {
            Name = "cardiology"
        };

        var result = await controller.PostSpecialization(specialization);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PutSpecialization_ValidSpecialization_ReturnsNoContent()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Specializations.Add(new Specialization
        {
            SpecializationId = 1,
            Name = "Cardiology"
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var controller = new SpecializationsController(context);

        var updatedSpecialization = new Specialization
        {
            SpecializationId = 1,
            Name = "Updated Cardiology"
        };

        var result = await controller.PutSpecialization(1, updatedSpecialization);

        Assert.IsType<NoContentResult>(result);

        var specializationFromDb = await context.Specializations.FindAsync(1);
        Assert.NotNull(specializationFromDb);
        Assert.Equal("Updated Cardiology", specializationFromDb.Name);
    }

    [Fact]
    public async Task DeleteSpecialization_WithDoctor_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new SpecializationsController(context);

        var result = await controller.DeleteSpecialization(1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetDoctorsBySpecialization_MissingSpecialization_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new SpecializationsController(context);

        var result = await controller.GetDoctorsBySpecialization(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}