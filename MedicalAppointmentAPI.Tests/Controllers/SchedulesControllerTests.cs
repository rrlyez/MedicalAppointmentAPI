using MedicalAppointmentAPI.Controllers;
using MedicalAppointmentAPI.Models;
using MedicalAppointmentAPI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalAppointmentAPI.Tests.Controllers;

public class SchedulesControllerTests
{
    [Fact]
    public async Task PostSchedule_ValidSchedule_ReturnsCreatedAtAction()
    {
        await using var context = TestDbContextFactory.CreateContext();

        context.Specializations.Add(new Specialization
        {
            SpecializationId = 1,
            Name = "Cardiology"
        });

        context.Doctors.Add(new Doctor
        {
            DoctorId = 1,
            FullName = "John Smith",
            SpecializationId = 1
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var controller = new SchedulesController(context);

        var schedule = new Schedule
        {
            DoctorId = 1,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(15, 0, 0)
        };

        var result = await controller.PostSchedule(schedule);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdSchedule = Assert.IsType<Schedule>(createdResult.Value);

        Assert.Equal(nameof(SchedulesController.GetSchedule), createdResult.ActionName);
        Assert.Equal(DayOfWeek.Tuesday, createdSchedule.DayOfWeek);
        Assert.Equal(1, await context.Schedules.CountAsync());
    }

    [Fact]
    public async Task PostSchedule_StartTimeAfterEndTime_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new SchedulesController(context);

        var schedule = new Schedule
        {
            DoctorId = 1,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeSpan(16, 0, 0),
            EndTime = new TimeSpan(9, 0, 0)
        };

        var result = await controller.PostSchedule(schedule);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostSchedule_MissingDoctor_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new SchedulesController(context);

        var schedule = new Schedule
        {
            DoctorId = 999,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(15, 0, 0)
        };

        var result = await controller.PostSchedule(schedule);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostSchedule_OverlappingSchedule_ReturnsBadRequest()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new SchedulesController(context);

        var overlappingSchedule = new Schedule
        {
            DoctorId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0)
        };

        var result = await controller.PostSchedule(overlappingSchedule);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetSchedulesByDoctor_MissingDoctor_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new SchedulesController(context);

        var result = await controller.GetSchedulesByDoctor(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteSchedule_ExistingSchedule_ReturnsNoContent()
    {
        await using var context = TestDbContextFactory.CreateContext();
        await TestDbContextFactory.SeedBasicDataAsync(context);

        var controller = new SchedulesController(context);

        var result = await controller.DeleteSchedule(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await context.Schedules.CountAsync());
    }
}