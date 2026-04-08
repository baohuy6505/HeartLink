using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.Auth;

public class LoginRequest
{
    [Required]
    public string EmailOrPhone { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}