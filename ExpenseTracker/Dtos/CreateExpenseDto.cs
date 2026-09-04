namespace ExpenseTracker.Dtos;

public class CreateExpenseDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int CategoryId { get; set; }

}
