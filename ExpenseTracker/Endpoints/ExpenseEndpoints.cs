using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Endpoints;

public static class ExpenseEndpoints
{
    public static void MapExpenseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/expenses").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, AppDbContext dbContext) =>
        {
            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is null)
                return Results.Unauthorized();

            int userId = int.Parse(claim);
            var expenses = await dbContext.Expenses
                                .Where(e => e.UserId == userId)
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
                                .ToListAsync();
            return Results.Ok(expenses);
        });


    }
}
