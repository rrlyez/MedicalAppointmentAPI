using MedicalAppointmentAPI.Data;
using MedicalAppointmentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointmentAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SchedulesController : ControllerBase
{
    private readonly MedicalDbContext _context;

    public SchedulesController(MedicalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
    {
        return await _context.Schedules
            .AsNoTracking()
            .OrderBy(s => s.ScheduleId)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Schedule>> GetSchedule(int id)
    {
        var schedule = await _context.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ScheduleId == id);

        return schedule is null ? NotFound("Schedule was not found.") : schedule;
    }

    [HttpPost]
    public async Task<ActionResult<Schedule>> PostSchedule(Schedule schedule)
    {
        var validationResult = await ValidateScheduleAsync(schedule);
        if (validationResult is not null)
            return validationResult;

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSchedule), new { id = schedule.ScheduleId }, schedule);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutSchedule(int id, Schedule schedule)
    {
        if (id != schedule.ScheduleId)
            return BadRequest("Route id and schedule id do not match.");

        var exists = await _context.Schedules.AnyAsync(s => s.ScheduleId == id);
        if (!exists)
            return NotFound("Schedule was not found.");

        var validationResult = await ValidateScheduleAsync(schedule, id);
        if (validationResult is not null)
            return validationResult;

        _context.Entry(schedule).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule is null)
            return NotFound("Schedule was not found.");

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("doctor/{doctorId:int}")]
    public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedulesByDoctor(int doctorId)
    {
        var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId);
        if (!doctorExists)
            return NotFound("Doctor was not found.");

        return await _context.Schedules
            .AsNoTracking()
            .Where(s => s.DoctorId == doctorId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    private async Task<BadRequestObjectResult?> ValidateScheduleAsync(Schedule schedule, int? currentScheduleId = null)
    {
        if (schedule.StartTime >= schedule.EndTime)
            return BadRequest("Start time must be earlier than end time.");

        var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == schedule.DoctorId);
        if (!doctorExists)
            return BadRequest("Selected doctor does not exist.");

        var overlaps = await _context.Schedules.AnyAsync(s =>
            s.DoctorId == schedule.DoctorId &&
            s.DayOfWeek == schedule.DayOfWeek &&
            (!currentScheduleId.HasValue || s.ScheduleId != currentScheduleId.Value) &&
            schedule.StartTime < s.EndTime &&
            schedule.EndTime > s.StartTime);

        if (overlaps)
            return BadRequest("This schedule overlaps with another schedule of the same doctor.");

        return null;
    }
}
