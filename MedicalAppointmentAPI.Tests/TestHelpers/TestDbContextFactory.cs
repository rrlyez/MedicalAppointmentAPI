using System.Numerics;
using MedicalAppointmentAPI.Data;
using MedicalAppointmentAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentAPI.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static MedicalDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MedicalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MedicalDbContext(options);
    }

    public static async Task SeedBasicDataAsync(MedicalDbContext context)
    {
        var specialization = new Specialization
        {
            SpecializationId = 1,
            Name = "Cardiology"
        };

        var doctor = new Doctor
        {
            DoctorId = 1,
            FullName = "John Smith",
            SpecializationId = 1
        };

        var patient = new Patient
        {
            PatientId = 1,
            FullName = "Anna Brown",
            Phone = "+380501112233",
            Email = "anna@example.com"
        };

        var schedule = new Schedule
        {
            ScheduleId = 1,
            DoctorId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        };

        context.Specializations.Add(specialization);
        context.Doctors.Add(doctor);
        context.Patients.Add(patient);
        context.Schedules.Add(schedule);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
    }

    public static DateTimeOffset FutureMondayAt(int hour, int minute = 0)
    {
        var now = DateTimeOffset.Now;

        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;

        if (daysUntilMonday == 0)
        {
            daysUntilMonday = 7;
        }

        var targetDateTime = now.Date
            .AddDays(daysUntilMonday)
            .AddHours(hour)
            .AddMinutes(minute);

        return new DateTimeOffset(targetDateTime, now.Offset);
    }
}