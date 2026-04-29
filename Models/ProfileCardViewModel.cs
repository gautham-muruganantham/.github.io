namespace Learning.Models;

public class ProfileCardViewModel
{
    public required User User { get; set; }
    public required Profile Profile { get; set; }
    public bool IsInterestSent { get; set; }
    public bool IsShortlisted { get; set; }
}
