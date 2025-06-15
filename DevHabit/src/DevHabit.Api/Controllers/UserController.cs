using System.Net.Mime;
using System.Security.Claims;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public sealed class UserController(
    ApplicationDbContext dbContext,
    UserContext userContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (id != userId)
            return Forbid();

        var user = await dbContext.Users
            .Where(u => u.Id == id)
            .Select(UserQueries.ToDto()) 
            .FirstOrDefaultAsync();

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var userDto = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(UserQueries.ToDto())
            .FirstOrDefaultAsync();

        if (userDto is null)
            return NotFound();

        return Ok(userDto);
    }
}
