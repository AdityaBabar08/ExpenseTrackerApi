using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTracker.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {

        app.MapPost("/register", async (RegisterUserDto registerUser, AppDbContext dbContext) =>
        {

            if (await dbContext.Users.AnyAsync(u => u.UserName == registerUser.Username || u.Email == registerUser.Email))
            {
                return Results.Conflict("User already exists");
            }

            var newUser = new User
            {
                UserName = registerUser.Username,
                Email = registerUser.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUser.Password),

            };

            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new ResponseUserDto
            {
                Id = newUser.UserId,
                Username = newUser.UserName,
                Email = newUser.Email

            });

        });

        app.MapPost("/login", async (LoginDto loginDto, AppDbContext dbContext, IConfiguration config) =>
        {
            string tokenString = string.Empty;
            var storedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.Username);
            if (storedUser is null)
            {
                return Results.Unauthorized();
            }
            bool isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, storedUser.PasswordHash);
            if (isValid)
            {
                var jwtConfigs = config.GetSection("jwt");
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, storedUser.UserId.ToString()),
                    new Claim(ClaimTypes.Name, storedUser.UserName)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfigs["key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtConfigs["Issuer"],
                    audience: jwtConfigs["audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(double.Parse(jwtConfigs["expireDays"])),
                    signingCredentials: creds
                );

                tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Results.Ok(new { tokenString });
            }
            else
            {
                return Results.Unauthorized();
            }

        });

    }

}
