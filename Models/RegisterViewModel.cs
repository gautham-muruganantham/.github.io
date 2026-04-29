using System.ComponentModel.DataAnnotations;

namespace Learning.Models;

public class RegisterViewModel
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = "Male";

    [Range(18, 80)]
    public int Age { get; set; } = 25;

    [Required]
    public string Religion { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Profession { get; set; } = string.Empty;
}
