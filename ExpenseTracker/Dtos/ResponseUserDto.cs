using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dtos;

public class ResponseUserDto
{
    [Required] public int Id { get; set; }
    [Required] public string Username = string.Empty;
    [Required][EmailAddress] public string Email = string.Empty;

}
