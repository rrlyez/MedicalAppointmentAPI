using System.ComponentModel.DataAnnotations;

namespace MedicalAppointmentAPI.Models;

public class Patient
{
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}