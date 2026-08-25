using System.ComponentModel.DataAnnotations;
using System.Text;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

app.Run();
