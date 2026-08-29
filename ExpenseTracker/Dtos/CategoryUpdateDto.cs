using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dtos;

public class CategoryUpdateDto
{
    [Required] public string UpdatedCategory = string.Empty;

}
