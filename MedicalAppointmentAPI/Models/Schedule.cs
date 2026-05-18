using System.ComponentModel.DataAnnotations;

namespace MedicalAppointmentAPI.Models;

public class Schedule
{
    public int ScheduleId { get; set; }

    [Required(ErrorMessage = "Doctor is required")]
    public int DoctorId { get; set; }

    public Doctor? Doctor { get; set; }

    [Required(ErrorMessage = "Day of week is required")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "Start time is required")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "End time is required")]
    public TimeSpan EndTime { get; set; }
}