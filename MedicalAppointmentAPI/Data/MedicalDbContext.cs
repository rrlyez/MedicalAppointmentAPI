using Microsoft.EntityFrameworkCore;
using MedicalAppointmentAPI.Models;

namespace MedicalAppointmentAPI.Data;

public class MedicalDbContext : DbContext
{
    public MedicalDbContext(DbContextOptions<MedicalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
}