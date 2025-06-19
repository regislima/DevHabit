using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.GitHub;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Member)]
[Route("github")]
public sealed class GitHubController(
    GitHubAccessTokenService gitHubAccessTokenService,
    GitHubService gitHubService,
    UserContext userContext,
    LinkTools linkTools) : ControllerBase
{
    [HttpPut("personal-access-token")]
    public async Task<IActionResult> StoreAccessToken(StoreGitHubAccessTokenDto storeGitHubAccessTokenDto)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await gitHubAccessTokenService.StoreAsync(userId, storeGitHubAccessTokenDto);

        return NoContent();
    }

    [HttpDelete("personal-access-token")]
    public async Task<IActionResult> RevokeAccessToken()
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await gitHubAccessTokenService.RevokeAsync(userId);

        return NoContent();
    }

    [HttpGet("profile")]
    public async Task<ActionResult<GitHubUserProfileDto>> GetUserProfile([FromHeader] AcceptHeaderDto acceptHeaderDto)
    {
        var userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var accessToken = await gitHubAccessTokenService.GetAsync(userId);

        if (string.IsNullOrWhiteSpace(accessToken))
            return NotFound();

        var userProfile = await gitHubService.GetUserProfileAsync(accessToken);

        if (userProfile is null)
            return NotFound();

        if (acceptHeaderDto.IncludeLinks)
            userProfile.Links =
            [
                linkTools.Create(nameof(GetUserProfile), "self", HttpMethods.Get),
                linkTools.Create(nameof(StoreAccessToken), "store-token", HttpMethods.Put),
                linkTools.Create(nameof(RevokeAccessToken), "revoke-token", HttpMethods.Delete),
            ];

        return Ok(userProfile);
    }
}
