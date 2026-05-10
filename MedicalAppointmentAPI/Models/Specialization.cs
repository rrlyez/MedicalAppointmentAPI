namespace MedicalAppointmentAPI.Models;

public class Specialization
{
    public int SpecializationId { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}

