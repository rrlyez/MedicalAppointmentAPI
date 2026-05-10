namespace MedicalAppointmentAPI.Models;

public class Patient
{
    public int PatientId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}