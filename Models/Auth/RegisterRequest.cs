using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.Auth;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = null!;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = null!;

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    public DateTime BirthDate { get; set; }

    [Required, RegularExpression("Nam|Nữ|Khác")]
    public string Gender { get; set; } = null!;

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    [MaxLength(255)]
    public string? Avatar { get; set; }

    [RegularExpression("Nam|Nữ|Khác|Tất cả")]
    public string TargetGender { get; set; } = "Tất cả";

    [Range(18, 99)]
    public int MinAge { get; set; } = 18;

    [Range(18, 99)]
    public int MaxAge { get; set; } = 99;

    [Range(1, 500)]
    public double Radius { get; set; } = 50;

    public List<string> Interests { get; set; } = new();
}