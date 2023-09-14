using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlestheticApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace AlestheticApi.Endpoints;

public static class AuthenticationEndpoint
{
    public static void ConfigureAuthenticationEndpoint(this WebApplication app, IConfiguration configuration)
    {
        app.MapPost("/api/auth/token", (UserDTO user) =>
        {
            if (user.Username == "Hcodes" && user.Password == "123456")
            {
                var issuer = configuration["Jwt:Issuer"];
                var audience = configuration["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
             }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                return Results.Ok(stringToken);
            }

            return Results.Unauthorized();
        });

        app.MapGet("/api/auth/refresh", () => "refresh")
        .RequireAuthorization(); 
    }
}