namespace Learning.Models;

public class InterestRecord
{
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public string Type { get; set; } = "interest";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
