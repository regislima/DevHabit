using System.Net.Mime;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using DevHabit.Api.Tools;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Member)]
[Route("tags")]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1)]
public sealed class TagsController(
    ApplicationDbContext dbContext,
    LinkTools linkTools,
    UserContext userContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags([FromHeader] AcceptHeaderDto acceptHeader)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var tags = await dbContext.Tags
            .Where(t => t.UserId == userId)
            .Select(t => t.ToDto())
            .ToListAsync();

        var habitsCollectionDto = new TagsCollectionDto
        {
            Items = tags
        };

        if (acceptHeader.IncludeLinks)
            habitsCollectionDto.Links = CreateLinksForTags(tags.Count);

        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id, [FromHeader] AcceptHeaderDto acceptHeader)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var tag = await dbContext.Tags
            .Where(h => h.Id == id && h.UserId == userId)
            .Select(t => t.ToDto())
            .FirstOrDefaultAsync();

        if (tag is null)
            return NotFound();

        if (acceptHeader.IncludeLinks)
            tag.Links = CreateLinksForTag(id);

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(
        CreateTagDto createTagDto,
        IValidator<CreateTagDto> validator,
        ProblemDetailsFactory problemDetailsFactory)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var validationResult = await validator.ValidateAsync(createTagDto);

        if (!validationResult.IsValid)
        {
            var problem = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest);
            problem.Extensions.Add("errors", validationResult.ToDictionary());

            return BadRequest(problem);
        }

        var tag = createTagDto.ToEntity(userId);

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
            return Problem(
                detail: $"The tag '{tag.Name}' already exists",
                statusCode: StatusCodes.Status409Conflict);

        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        var tagDto = tag.ToDto();

        return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, UpdateTagDto updateTagDto)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

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
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var tag = await dbContext.Tags.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (tag is null)
            return NotFound();

        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForTags(int tagsCount)
    {
        List<LinkDto> links =
        [
            linkTools.Create(nameof(GetTags), "self", HttpMethods.Get),
        ];

        if (tagsCount < 5)
            links.Add(linkTools.Create(nameof(CreateTag), "create", HttpMethods.Post));

        return links;
    }

    private List<LinkDto> CreateLinksForTag(string id)
    {
        List<LinkDto> links =
        [
            linkTools.Create(nameof(GetTag), "self", HttpMethods.Get, new { id }),
            linkTools.Create(nameof(UpdateTag), "update", HttpMethods.Put, new { id }),
            linkTools.Create(nameof(DeleteTag), "delete", HttpMethods.Delete, new { id })
        ];

        return links;
    }
}
