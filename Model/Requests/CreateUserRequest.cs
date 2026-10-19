using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public class CreateUserRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Fullname { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string RePassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}