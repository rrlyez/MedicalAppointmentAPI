using MedicalAppointmentAPI.Data;
using MedicalAppointmentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public PatientsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
    {
        return await _context.Patients
            .AsNoTracking()
            .OrderBy(p => p.PatientId)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Patient>> GetPatient(int id)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == id);

        return patient is null ? NotFound("Patient was not found.") : patient;
    }

    [HttpPost]
    public async Task<ActionResult<Patient>> PostPatient(Patient patient)
    {
        if (!string.IsNullOrWhiteSpace(patient.Email))
        {
            var emailExists = await _context.Patients
                .AnyAsync(p => p.Email != null && p.Email.ToLower() == patient.Email.ToLower());

            if (emailExists)
                return BadRequest("Email is already in use.");
        }

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId }, patient);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutPatient(int id, Patient patient)
    {
        if (id != patient.PatientId)
            return BadRequest("Route id and patient id do not match.");

        var exists = await _context.Patients.AnyAsync(p => p.PatientId == id);
        if (!exists)
            return NotFound("Patient was not found.");

        if (!string.IsNullOrWhiteSpace(patient.Email))
        {
            var emailExists = await _context.Patients
                .AnyAsync(p => p.PatientId != id && p.Email != null && p.Email.ToLower() == patient.Email.ToLower());

            if (emailExists)
                return BadRequest("Email is already in use.");
        }

        _context.Entry(patient).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient is null)
            return NotFound("Patient was not found.");

        var hasAppointments = await _context.Appointments.AnyAsync(a => a.PatientId == id);
        if (hasAppointments)
            return BadRequest("Patient cannot be deleted because he/she has appointments.");

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Patient>>> SearchPatients([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Search name is required.");

        return await _context.Patients
            .AsNoTracking()
            .Where(p => p.FullName.ToLower().Contains(name.ToLower()))
            .OrderBy(p => p.FullName)
            .ToListAsync();
    }
}
