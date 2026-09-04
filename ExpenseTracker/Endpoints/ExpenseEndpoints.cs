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

        group.MapPatch("/{id:int}", async (int id, UpdateExpenseDto updateDto, ClaimsPrincipal user, AppDbContext dbContext) =>
        {
            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Results.Unauthorized();

            int userId = int.Parse(claim);
            var expense = await dbContext.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);

            if (expense is null)
                return Results.NotFound();

            if (updateDto.CategoryId.HasValue)
            {
                var category = await dbContext.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == updateDto.CategoryId.Value && c.UserId == userId);

                if (category is null)
                    return Results.BadRequest($"Category '{updateDto.CategoryId}' does not exist.");

                expense.CategoryId = category.CategoryId;
                expense.Category = category;
            }

            if (updateDto.Title is not null) expense.Title = updateDto.Title;
            if (updateDto.Description is not null) expense.Description = updateDto.Description;
            if (updateDto.Amount.HasValue) expense.Amount = updateDto.Amount.Value;
            if (updateDto.ExpenseDate.HasValue) expense.ExpenseDate = updateDto.ExpenseDate.Value;

            await dbContext.SaveChangesAsync();

            return Results.Ok(new ExpenseResponseDto
            {
                ExpenseId = expense.ExpenseId,
                Title = expense.Title,
                Description = expense.Description,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                CreatedAt = expense.CreatedAt,
                CategoryName = expense.Category.Name
            });
        });

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext dbContext) =>
        {
            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Results.Unauthorized();

            int userId = int.Parse(claim);
            var expense = await dbContext.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);

            if (expense is null)
                return Results.NotFound();

            dbContext.Expenses.Remove(expense);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapGet("/summary", async (string? category, DateTime? startdate, DateTime? enddate, ClaimsPrincipal user, AppDbContext dbContext) =>
        {
            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Results.Unauthorized();

            int userId = int.Parse(claim);
            var query = dbContext.Expenses.Where(e => e.UserId == userId);

            if (category is not null)
                query = query.Where(e => e.Category.Name == category);

            if (startdate.HasValue)
                query = query.Where(e => e.ExpenseDate >= startdate.Value.ToUniversalTime());

            if (enddate.HasValue)
                query = query.Where(e => e.ExpenseDate <= enddate.Value.ToUniversalTime());

            var total = await query.SumAsync(e => e.Amount);
            var count = await query.CountAsync();

            return Results.Ok(new
            {
                TotalAmount = total,
                ExpenseCount = count,
                Category = category,
                StartDate = startdate,
                EndDate = enddate
            });
        });
    }
}
