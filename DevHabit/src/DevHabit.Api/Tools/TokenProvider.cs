using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevHabit.Api.Tools;

public sealed class TokenProvider(IOptions<JwtAuthOptions> options)
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public AccessTokenDto Create(TokenRequest tokenRequest) =>
        new(GenerateAccessToken(tokenRequest), GenerateRefreshToken());

    private string GenerateAccessToken(TokenRequest tokenRequest)
    {
        var securiryKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptions.Key));
        var credentials = new SigningCredentials(securiryKey, SecurityAlgorithms.HmacSha256);
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, tokenRequest.UserId),
            new(JwtRegisteredClaimNames.Email, tokenRequest.Email),
            ..tokenRequest.Roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtAuthOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtAuthOptions.Issuer,
            Audience = _jwtAuthOptions.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(accessToken);
    }

    private static string GenerateRefreshToken()
    {
        var ramdomBytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(ramdomBytes);
    }
}
