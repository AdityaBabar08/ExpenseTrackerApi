using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models;

public class User
{
    public int UserId { get; set; }
    [Required] public string UserName { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
