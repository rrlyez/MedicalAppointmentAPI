namespace MedicalAppointmentAPI.Models;

public class Doctor
{
    public int DoctorId { get; set; }

    public string FullName { get; set; } = null!;

    public int SpecializationId { get; set; }

    public Specialization Specialization { get; set; } = null!;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
