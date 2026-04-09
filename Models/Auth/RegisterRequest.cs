using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.Auth;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = null!;

    [Required, Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = null!;

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    public DateTime BirthDate { get; set; }

    [Required, RegularExpression("Nam|Nữ|Khác")]
    public string Gender { get; set; } = null!;
}