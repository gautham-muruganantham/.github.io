using System.ComponentModel.DataAnnotations;

namespace Learning.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = "Male";

    [Range(18, 80)]
    public int Age { get; set; } = 25;
}
