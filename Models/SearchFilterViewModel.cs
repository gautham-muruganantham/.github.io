namespace Learning.Models;

public class SearchFilterViewModel
{
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 80;
    public string? City { get; set; }
    public string? Religion { get; set; }
}
