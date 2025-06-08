using System.Net.Mime;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public sealed class UserController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        var user = await dbContext.Users
            .Where(u => u.Id == id)
            .Select(UserQueries.ToDto())
            .FirstOrDefaultAsync();

        if (user is null)
            return NotFound();

        return Ok(user);
    }
}
