using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

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

app.Run();
