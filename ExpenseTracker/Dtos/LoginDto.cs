using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dtos;

public class LoginDto
{
    [Required] public string Username = string.Empty;
    [Required] public string Password = string.Empty;
}
