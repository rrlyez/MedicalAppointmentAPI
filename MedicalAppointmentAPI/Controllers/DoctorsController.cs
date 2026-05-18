using MedicalAppointmentAPI.Data;
using MedicalAppointmentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public DoctorsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
    {
        return await _context.Doctors
            .AsNoTracking()
            .OrderBy(d => d.DoctorId)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Doctor>> GetDoctor(int id)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DoctorId == id);

        return doctor is null ? NotFound("Doctor was not found.") : doctor;
    }

    [HttpPost]
    public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
    {
        var specializationExists = await _context.Specializations
            .AnyAsync(s => s.SpecializationId == doctor.SpecializationId);

        if (!specializationExists)
            return BadRequest("Selected specialization does not exist.");

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.FullName.ToLower() == doctor.FullName.ToLower()
                        && d.SpecializationId == doctor.SpecializationId);

        if (doctorExists)
            return BadRequest("This doctor already exists in the selected specialization.");

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, doctor);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutDoctor(int id, Doctor doctor)
    {
        if (id != doctor.DoctorId)
            return BadRequest("Route id and doctor id do not match.");

        var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == id);
        if (!doctorExists)
            return NotFound("Doctor was not found.");

        var specializationExists = await _context.Specializations
            .AnyAsync(s => s.SpecializationId == doctor.SpecializationId);

        if (!specializationExists)
            return BadRequest("Selected specialization does not exist.");

        var duplicateDoctor = await _context.Doctors
            .AnyAsync(d => d.DoctorId != id
                        && d.FullName.ToLower() == doctor.FullName.ToLower()
                        && d.SpecializationId == doctor.SpecializationId);

        if (duplicateDoctor)
            return BadRequest("This doctor already exists in the selected specialization.");

        _context.Entry(doctor).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor is null)
            return NotFound("Doctor was not found.");

        var hasAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == id);
        if (hasAppointments)
            return BadRequest("Doctor cannot be deleted because he/she has appointments.");

        var hasSchedules = await _context.Schedules.AnyAsync(s => s.DoctorId == id);
        if (hasSchedules)
            return BadRequest("Doctor cannot be deleted because he/she has schedules.");

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("specialization/{specializationId:int}")]
    public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctorsBySpecialization(int specializationId)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Where(d => d.SpecializationId == specializationId)
            .OrderBy(d => d.FullName)
            .ToListAsync();
    }
}
