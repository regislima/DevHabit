using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq.Expressions;
using DevHabit.Api.Database;
using DevHabit.Api.Database.SortMapping;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using DevHabit.Api.Extensions;
using DevHabit.Api.Tools;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext, LinkTools linkTools) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingTool dataShapingTool)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
            return Problem(
                detail: $"Invalid sort parameter. '{query.Sort}'.",
                statusCode: StatusCodes.Status400BadRequest);

        if (!dataShapingTool.Validate<HabitDto>(query.Fields))
            return Problem(
                detail: $"Invalid data shaping fields. '{query.Fields}'.",
                statusCode: StatusCodes.Status400BadRequest);

        query.Search ??= query.Search?.Trim().ToLower();
        var sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();
        var habitsQuery = dbContext
            .Habits
            .Where(h => query.Search == null ||
                   h.Name.Contains(query.Search) ||
                   h.Description != null && h.Description.Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            .ApplySort(query.Sort, sortMappings)
            .Select(h => h.ToDto());

        var totalCount = await habitsQuery.CountAsync();
        var habits = await habitsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingTool.ShapeCollectionData(
                habits,
                query.Fields,
                h => CreateLinkForHabit(h.Id, query.Fields)),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
        paginationResult.Links = CreateLinksForHabits(
            query,
            paginationResult.HasNextPage,
            paginationResult.HasPreviousPage);

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabit(
        string id,
        string? fields,
        DataShapingTool dataShapingTool)
    {
        if (!dataShapingTool.Validate<HabitWithTagsDto>(fields))
            return Problem(
                detail: $"Invalid data shaping fields. '{fields}'.",
                statusCode: StatusCodes.Status400BadRequest);

        var habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(h => h.ToDto())
            .FirstOrDefaultAsync();

        if (habit is null)
            return NotFound();

        var shapedHabitDto = dataShapingTool.ShapeData(habit, fields);
        var linksDto = CreateLinkForHabit(id, fields);

        shapedHabitDto.TryAdd("links", linksDto);

        return Ok(shapedHabitDto);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(
        CreateHabitDto createHabitDto,
        IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);
        var habit = createHabitDto.ToEntity();
        
        await dbContext.Habits.AddAsync(habit);
        await dbContext.SaveChangesAsync();
        var habitDto = habit.ToDto();
        habitDto.Links = CreateLinkForHabit(habit.Id, null);

        return CreatedAtAction(nameof(GetHabit),
            new { id = habit.Id },
            habitDto);
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

    private List<LinkDto> CreateLinksForHabits(
        HabitsQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links =
            [
                linkTools.Create(nameof(GetHabits), "self", HttpMethods.Get, new
                {
                    page = parameters.Page,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                    type = parameters.Type,
                    status = parameters.Status
                }),
                linkTools.Create(nameof(CreateHabit), "create", HttpMethods.Post)
            ];

        if (hasNextPage)
        {
            links.Add(linkTools.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkTools.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        return links;
    }

    private List<LinkDto> CreateLinkForHabit(string id, string? fields) =>
        [
            linkTools.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id, fields }),
            linkTools.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new { id }),
            linkTools.Create(nameof(PatchHabit), "partial-update", HttpMethods.Patch, new { id }),
            linkTools.Create(nameof(DeleteHabit), "delete", HttpMethods.Delete, new { id }),
            linkTools.Create(
                nameof(HabitTagsController.UpsertTagToHabit),
                "upsert-tags",
                HttpMethods.Put,
                new { habitId = id },
                HabitTagsController.Name)
        ];
}
