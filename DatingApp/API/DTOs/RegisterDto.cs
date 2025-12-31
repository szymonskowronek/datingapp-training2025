using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = "";
    [Required]
    [EmailAddress]  
    public string Email { get; set; } = "";
    [Required]
    [MinLength(4)]
    [MaxLength(8)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string Gender { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string Country { get; set; } = string.Empty;
    [Required]
    public DateOnly DateOfBirth { get; set; }
    
}
