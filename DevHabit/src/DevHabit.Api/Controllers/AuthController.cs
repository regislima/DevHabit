using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Settings;
using DevHabit.Api.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace DevHabit.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext applicationDbContext,
    TokenProvider tokenProvider,
    IOptions<JwtAuthOptions> options) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
    {
        using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
        applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        var identityUser = new IdentityUser
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.Email
        };
        
        var identityResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);

        if (!identityResult.Succeeded)
        {
            var extensions = new Dictionary<string, object?>
            {
                {
                    "errors",
                    identityResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                }
            };

            return Problem(
                detail: "Unable to register user, please t again.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extensions);
        }

        var user = registerUserDto.ToEntity();
        user.IdentityId = identityUser.Id;
        applicationDbContext.Users.Add(user);
        await applicationDbContext.SaveChangesAsync();
        
        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email);
        var accessTokenDto = tokenProvider.Create(tokenRequest);
        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokenDto.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(options.Value.RefreshTokenExpirationDays)
        };

        identityDbContext.RefreshTokens.Add(refreshToken);
        await identityDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(accessTokenDto);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenDto>> Login(LoginUserDto loginUserDto)
    {
        var identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);

        if (identityUser is null || !await userManager.CheckPasswordAsync(identityUser, loginUserDto.Password))
            return Unauthorized();

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!);
        var accessTokenDto = tokenProvider.Create(tokenRequest);
        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokenDto.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(options.Value.RefreshTokenExpirationDays)
        };

        identityDbContext.RefreshTokens.Add(refreshToken);
        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokenDto);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenDto>> Refresh(RefreshTokenDto refreshTokenDto)
    {
        var refreshToken = await identityDbContext
            .RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

        if (refreshToken is null || refreshToken.ExpiresAtUtc < DateTime.UtcNow)
            return Unauthorized();

        var tokenRequest = new TokenRequest(refreshToken.User.Id, refreshToken.User.Email!);
        var accessTokenDto = tokenProvider.Create(tokenRequest);
        refreshToken.Token = accessTokenDto.RefreshToken;
        refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(options.Value.RefreshTokenExpirationDays);
        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokenDto);
    }
}
