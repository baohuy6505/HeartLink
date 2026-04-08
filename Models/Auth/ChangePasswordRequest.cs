using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.Auth;

public class ChangePasswordRequest
{
    [Required]
    public string OldPassword { get; set; } = null!;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = null!;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = null!;
}