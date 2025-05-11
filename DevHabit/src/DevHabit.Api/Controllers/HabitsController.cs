using System.ComponentModel.DataAnnotations;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.Tags;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits()
    {
        var habits = await dbContext
            .Habits
            .Select(h => h.ToDto())
            .ToListAsync();
        
        return Ok(new HabitsCollectionDto(habits));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id)
    {
        var habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(h => h.ToDto())
            .FirstOrDefaultAsync();

        if (habit is null)
            return NotFound();

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(
        CreateHabitDto createHabitDto,
        IValidator<CreateHabitDto> validator,
        ProblemDetailsFactory problemDetailsFactory)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);
        var habit = createHabitDto.ToEntity();
        
        await dbContext.Habits.AddAsync(habit);
        await dbContext.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetHabit),
            new { id = habit.Id },
            habit.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, [FromBody] UpdateHabitDto updateHabitDto)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        
        if (habit is null)
            return NotFound();
        
        habit.UpdateFromDto(updateHabitDto);
        dbContext.Habits.Update(habit);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, [FromBody] JsonPatchDocument<HabitDto> patchDocument)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
            return NotFound();

        var habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
            return ValidationProblem(ModelState);

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.Update(habit);
        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        
        if (habit is null)
            return NotFound();
        
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }
}
