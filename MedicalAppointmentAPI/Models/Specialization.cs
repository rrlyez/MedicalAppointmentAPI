using System.ComponentModel.DataAnnotations;

namespace MedicalAppointmentAPI.Models;

public class Specialization
{
    public int SpecializationId { get; set; }

    [Required(ErrorMessage = "Specialization name is required")]
    [StringLength(50, ErrorMessage = "Specialization name cannot exceed 50 characters")]
    public string Name { get; set; } = null!;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}