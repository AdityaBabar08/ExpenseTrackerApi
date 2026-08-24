using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dtos;

public class RegisterUserDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
