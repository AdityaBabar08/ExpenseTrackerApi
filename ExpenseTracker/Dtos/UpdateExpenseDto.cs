namespace ExpenseTracker.Dtos;

public class UpdateExpenseDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? ExpenseDate { get; set; }
    public int? CategoryId { get; set; }
}
