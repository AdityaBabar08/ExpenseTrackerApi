using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/categories").RequireAuthorization();

        group.MapGet("/", async (AppDbContext dbContext, ClaimsPrincipal user) =>
        {
            int userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var query = dbContext.Categories.Where(t => t.UserId == userId);
            return await query.ToListAsync();
        }).RequireAuthorization();

        group.MapPost("/", async (CategoryCreateDto categoryCreateDto, AppDbContext dbContext, ClaimsPrincipal user) =>
        {
            int userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            bool exist = await dbContext.Categories.AnyAsync(c => c.Name.ToLower() == categoryCreateDto.CategoryName.ToLower() && c.UserId == userId);
            if (exist)
            {
                return Results.Conflict("Category already exist");
            }
            Category newCategory = new()
            {
                Name = categoryCreateDto.CategoryName,
                UserId = userId
            };
            await dbContext.Categories.AddAsync(newCategory);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/categories/{newCategory.CategoryId}", newCategory);

        }).RequireAuthorization();

        group.MapPatch("/{id}", async (int id, CategoryUpdateDto categoryUpdateDto, AppDbContext dbContext, ClaimsPrincipal user) =>
        {

            int userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
            if (existingCategory is null)
            {
                return Results.NotFound("Category of this Id not found or doesn't exist");
            }
            existingCategory.Name = categoryUpdateDto.UpdatedCategory;

            await dbContext.SaveChangesAsync();
            return Results.Ok("Category updated successfully");

        }).RequireAuthorization();

        group.MapDelete("/{id}", async (int id, AppDbContext dbContext, ClaimsPrincipal user) =>
        {
            int userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
            if (existingCategory is null)
            {
                return Results.NotFound("Category of this Id not found or doesn't exist");
            }
            dbContext.Categories.Remove(existingCategory);

            await dbContext.SaveChangesAsync();
            return Results.NoContent();

        }).RequireAuthorization();
    }

}
