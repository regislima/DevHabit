using DevHabit.Api.Database;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Authorize(Roles = Roles.Member)]
[Route("habits/{habitId}/tags")]
[Authorize]
public sealed class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{
    public static readonly string Name = nameof(HabitTagsController).Replace("Controller", string.Empty);

    [HttpPut]
    public async Task<ActionResult> UpsertTagToHabit(string habitId, [FromBody] UpsertHabitTagsDto upsertHabitTagsDto)
    {
        var habit = await dbContext.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId);

        if (habit is null)
            return NotFound();

        var currentTagIds = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();

        if (currentTagIds.SetEquals(upsertHabitTagsDto.TagIds))
            return NoContent();

        var existingtagIds = await dbContext.Tags
            .Where(t => upsertHabitTagsDto.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        if (existingtagIds.Count != upsertHabitTagsDto.TagIds.Count)
            return BadRequest("One or more tags IDs is invalid.");

        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));
        var tagIdsToAdd = upsertHabitTagsDto.TagIds.Except(currentTagIds).ToArray();
        habit.HabitTags.AddRange(tagIdsToAdd.Select(tagId => new HabitTag
        {
            HabitId = habitId,
            TagId = tagId,
            CreatedAtUtc = DateTime.UtcNow
        }));

        await dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTag(string habitId, string tagId)
    {
        var habitTag = await dbContext.HabitTags
            .FirstOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);

        if (habitTag is null)
            return NotFound();

        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
