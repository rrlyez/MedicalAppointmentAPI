using MedicalAppointmentAPI.Data;
using MedicalAppointmentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpecializationsController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public SpecializationsController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Specialization>>> GetSpecializations()
    {
        return await _context.Specializations
            .AsNoTracking()
            .OrderBy(s => s.SpecializationId)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Specialization>> GetSpecialization(int id)
    {
        var specialization = await _context.Specializations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SpecializationId == id);

        return specialization is null ? NotFound("Specialization was not found.") : specialization;
    }

    [HttpPost]
    public async Task<ActionResult<Specialization>> PostSpecialization(Specialization specialization)
    {
        var exists = await _context.Specializations
            .AnyAsync(s => s.Name.ToLower() == specialization.Name.ToLower());

        if (exists)
            return BadRequest("Specialization already exists.");

        _context.Specializations.Add(specialization);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSpecialization), new { id = specialization.SpecializationId }, specialization);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutSpecialization(int id, Specialization specialization)
    {
        if (id != specialization.SpecializationId)
            return BadRequest("Route id and specialization id do not match.");

        var exists = await _context.Specializations.AnyAsync(s => s.SpecializationId == id);
        if (!exists)
            return NotFound("Specialization was not found.");

        var nameExists = await _context.Specializations
            .AnyAsync(s => s.SpecializationId != id && s.Name.ToLower() == specialization.Name.ToLower());

        if (nameExists)
            return BadRequest("Specialization already exists.");

        _context.Entry(specialization).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSpecialization(int id)
    {
        var specialization = await _context.Specializations.FindAsync(id);
        if (specialization is null)
            return NotFound("Specialization was not found.");

        var hasDoctors = await _context.Doctors.AnyAsync(d => d.SpecializationId == id);
        if (hasDoctors)
            return BadRequest("Specialization cannot be deleted because doctors use it.");

        _context.Specializations.Remove(specialization);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:int}/doctors")]
    public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctorsBySpecialization(int id)
    {
        var exists = await _context.Specializations.AnyAsync(s => s.SpecializationId == id);
        if (!exists)
            return NotFound("Specialization was not found.");

        return await _context.Doctors
            .AsNoTracking()
            .Where(d => d.SpecializationId == id)
            .OrderBy(d => d.FullName)
            .ToListAsync();
    }
}
