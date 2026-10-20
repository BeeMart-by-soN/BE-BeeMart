using System.ComponentModel.DataAnnotations;

namespace S1.Model.Requests;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name wajib diisi")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Re-Password wajib diisi")]
    public string RePassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role wajib dipilih")]
    public string Role { get; set; } = string.Empty;
}
