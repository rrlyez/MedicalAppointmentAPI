using MedicalAppointmentAPI.Data;
using MedicalAppointmentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public AppointmentsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
    {
        return await _context.Appointments
            .AsNoTracking()
            .OrderBy(a => a.AppointmentId)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment is null)
        {
            return NotFound("Appointment was not found.");
        }

        return appointment;
    }

    [HttpPost]
    public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
    {
        var validationResult = await ValidateAppointmentAsync(appointment);

        if (validationResult is not null)
        {
            return validationResult;
        }

        appointment.AppointmentDateTime = appointment.AppointmentDateTime.ToUniversalTime();

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAppointment),
            new { id = appointment.AppointmentId },
            appointment
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
    {
        if (id != appointment.AppointmentId)
        {
            return BadRequest("Route id and appointment id do not match.");
        }

        var exists = await _context.Appointments
            .AnyAsync(a => a.AppointmentId == id);

        if (!exists)
        {
            return NotFound("Appointment was not found.");
        }

        var validationResult = await ValidateAppointmentAsync(appointment, id);

        if (validationResult is not null)
        {
            return validationResult;
        }

        appointment.AppointmentDateTime = appointment.AppointmentDateTime.ToUniversalTime();

        _context.Entry(appointment).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment is null)
        {
            return NotFound("Appointment was not found.");
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("doctor/{doctorId:int}")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByDoctor(int doctorId)
    {
        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.DoctorId == doctorId);

        if (!doctorExists)
        {
            return NotFound("Doctor was not found.");
        }

        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByPatient(int patientId)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.PatientId == patientId);

        if (!patientExists)
        {
            return NotFound("Patient was not found.");
        }

        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeAppointmentStatus(int id, [FromBody] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest("Status is required.");
        }

        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment is null)
        {
            return NotFound("Appointment was not found.");
        }

        appointment.Status = status.Trim();
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<BadRequestObjectResult?> ValidateAppointmentAsync(
        Appointment appointment,
        int? currentAppointmentId = null)
    {
        if (appointment.AppointmentDateTime < DateTimeOffset.Now)
        {
            return BadRequest("Appointment date cannot be in the past.");
        }

        if (appointment.DurationMinutes <= 0)
        {
            return BadRequest("Appointment duration must be greater than zero.");
        }

        var patientExists = await _context.Patients
            .AnyAsync(p => p.PatientId == appointment.PatientId);

        if (!patientExists)
        {
            return BadRequest("Selected patient does not exist.");
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.DoctorId == appointment.DoctorId);

        if (!doctorExists)
        {
            return BadRequest("Selected doctor does not exist.");
        }

        var localAppointmentDateTime = appointment.AppointmentDateTime.ToLocalTime();

        var appointmentDay = localAppointmentDateTime.DayOfWeek;
        var appointmentStartTime = localAppointmentDateTime.TimeOfDay;
        var appointmentEndTime = appointmentStartTime.Add(
            TimeSpan.FromMinutes(appointment.DurationMinutes)
        );

        var doctorIsAvailable = await _context.Schedules.AnyAsync(s =>
            s.DoctorId == appointment.DoctorId &&
            s.DayOfWeek == appointmentDay &&
            appointmentStartTime >= s.StartTime &&
            appointmentEndTime <= s.EndTime
        );

        if (!doctorIsAvailable)
        {
            return BadRequest("Doctor does not work at this time.");
        }

        var newStart = appointment.AppointmentDateTime.ToUniversalTime();
        var newEnd = newStart.AddMinutes(appointment.DurationMinutes);

        var hasTimeConflict = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == appointment.DoctorId &&
            (!currentAppointmentId.HasValue || a.AppointmentId != currentAppointmentId.Value) &&
            newStart < a.AppointmentDateTime.AddMinutes(a.DurationMinutes) &&
            newEnd > a.AppointmentDateTime
        );

        if (hasTimeConflict)
        {
            return BadRequest("This appointment overlaps with another appointment of the same doctor.");
        }

        return null;
    }
}