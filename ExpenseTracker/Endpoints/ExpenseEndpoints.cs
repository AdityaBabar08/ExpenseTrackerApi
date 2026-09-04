using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.DTOs;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Endpoints;

public static class ExpenseEndpoints
{
    public static void MapExpenseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/expenses").RequireAuthorization();

        group.MapGet("/", async (string? category, ClaimsPrincipal user, AppDbContext dbContext) =>
        {
            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Results.Unauthorized();

            int userId = int.Parse(claim);
            var query = dbContext.Expenses.Where(e => e.UserId == userId);

            if (category is not null)
            {
                query = query.Where(e => e.Category.Name == category);
            }
            var expenses = await query.Select(e => new ExpenseResponseDto
            {
                ExpenseId = e.ExpenseId,
                Title = e.Title,
                Description = e.Description,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                CreatedAt = e.CreatedAt,
                CategoryName = e.Category.Name
            }).ToListAsync();
            return Results.Ok(expenses);
        });

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext dbContext) =>
        {
            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Results.Unauthorized();

            int userId = int.Parse(claim);
            var expense = await dbContext.Expenses
                                .Where(e => e.UserId == userId && e.ExpenseId == id)
                                .Select(e => new ExpenseResponseDto
                                {
                                    ExpenseId = e.ExpenseId,
                                    Title = e.Title,
                                    Description = e.Description,
                                    Amount = e.Amount,
                                    ExpenseDate = e.ExpenseDate,
                                    CreatedAt = e.CreatedAt,
                                    CategoryName = e.Category.Name
                                })
                                .FirstOrDefaultAsync();

            if (expense is null)
                return Results.NotFound();

            return Results.Ok(expense);

        });

        group.MapPost("/", async (CreateExpenseDto expenseDto, ClaimsPrincipal user, AppDbContext dbContext) =>
        {
            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Results.Unauthorized();

            int userId = int.Parse(claim);

            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == expenseDto.CategoryId && c.UserId == userId);
            if (category is null)
                return Results.BadRequest($"Category '{expenseDto.CategoryId}' does not exist.");

            var expense = new Expense
            {
                Title = expenseDto.Title,
                Description = expenseDto.Description,
                Amount = expenseDto.Amount,
                ExpenseDate = expenseDto.ExpenseDate,
                UserId = userId,
                CategoryId = category.CategoryId
            };

            dbContext.Expenses.Add(expense);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/expenses/{expense.ExpenseId}", new ExpenseResponseDto
            {
                ExpenseId = expense.ExpenseId,
                Title = expense.Title,
                Description = expense.Description,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                CreatedAt = expense.CreatedAt,
                CategoryName = category.Name
            });
        });

        // group.MapPatch()




    }
}
