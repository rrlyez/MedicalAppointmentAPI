namespace MedicalAppointmentAPI.Models;

public class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }

    public Doctor Doctor { get; set; } = null!;

    public DateTime AppointmentDateTime { get; set; }

    public string Status { get; set; } = "Scheduled";
}