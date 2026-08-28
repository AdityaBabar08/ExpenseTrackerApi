using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dtos;

public class CategoryCreateDto
{
    [Required] public string CategoryName = string.Empty;

}
