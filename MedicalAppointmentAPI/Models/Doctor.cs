using System.ComponentModel.DataAnnotations;

namespace MedicalAppointmentAPI.Models;

public class Doctor
{
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Doctor name is required")]
    [StringLength(100, ErrorMessage = "Doctor name cannot exceed 100 characters")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Specialization is required")]
    public int SpecializationId { get; set; }

    public Specialization? Specialization { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}