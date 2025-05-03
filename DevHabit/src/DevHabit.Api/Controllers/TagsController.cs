using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("tags")]
public sealed class TagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags()
    {
        var tags = await dbContext
            .Tags
            .Select(t => t.ToDto())
            .ToListAsync();

        var habitsCollectionDto = new TagsCollectionDto
        {
            Data = tags
        };

        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id)
    {
        var tag = await dbContext
            .Tags
            .Where(h => h.Id == id)
            .Select(t => t.ToDto())
            .FirstOrDefaultAsync();

        if (tag is null)
            return NotFound();

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto)
    {
        var tag = createTagDto.ToEntity();

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
            return Conflict($"The tag '{tag.Name}' already exists");

        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        var tagDto = tag.ToDto();

        return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, UpdateTagDto updateTagDto)
    {
        var tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id);

        if (tag is null)
            return NotFound();

        tag.UpdateFromDto(updateTagDto);
        dbContext.Tags.Update(tag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        var tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id);

        if (tag is null)
            return NotFound();

        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
