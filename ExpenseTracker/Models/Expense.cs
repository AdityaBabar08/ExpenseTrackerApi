using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models;

public class Expense
{
    public int ExpenseId { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public decimal Amount { get; set; }
    [Required] public DateTime ExpenseDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
