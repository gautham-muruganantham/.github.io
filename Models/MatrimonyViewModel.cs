namespace Learning.Models;

public class MatrimonyViewModel
{
    public SearchFilterViewModel Filter { get; set; } = new();
    public List<ProfileCardViewModel> Profiles { get; set; } = [];
    public int InterestCount { get; set; }
    public int ShortlistCount { get; set; }
}
