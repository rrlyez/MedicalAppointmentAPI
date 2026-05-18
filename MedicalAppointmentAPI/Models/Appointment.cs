using System.ComponentModel.DataAnnotations;

namespace MedicalAppointmentAPI.Models;

public class Appointment
{
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "Patient is required")]
    public int PatientId { get; set; }

    public Patient? Patient { get; set; }

    [Required(ErrorMessage = "Doctor is required")]
    public int DoctorId { get; set; }

    public Doctor? Doctor { get; set; }

    [Required(ErrorMessage = "Appointment date and time are required")]
    public DateTimeOffset AppointmentDateTime { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(30, ErrorMessage = "Status cannot exceed 30 characters")]
    public string Status { get; set; } = "Scheduled";
}