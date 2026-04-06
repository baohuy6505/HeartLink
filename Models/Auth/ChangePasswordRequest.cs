using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.Auth
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [MaxLength(100, ErrorMessage = "Mật khẩu mới tối đa 100 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu mới không được để trống")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}