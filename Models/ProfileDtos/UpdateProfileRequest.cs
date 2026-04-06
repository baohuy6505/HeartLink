using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.ProfileDtos
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        [StringLength(20, ErrorMessage = "Giới tính tối đa 20 ký tự")]
        public string? Gender { get; set; }

        [StringLength(1000, ErrorMessage = "Bio tối đa 1000 ký tự")]
        public string? Bio { get; set; }

        [StringLength(255, ErrorMessage = "Location tối đa 255 ký tự")]
        public string? Location { get; set; }

        [StringLength(500, ErrorMessage = "Avatar tối đa 500 ký tự")]
        public string? Avatar { get; set; }

        [StringLength(1000, ErrorMessage = "Interests tối đa 1000 ký tự")]
        public string? Interests { get; set; }

        [StringLength(20, ErrorMessage = "TargetGender tối đa 20 ký tự")]
        public string? TargetGender { get; set; }

        [Range(18, 100, ErrorMessage = "MinAge phải từ 18 đến 100")]
        public int? MinAge { get; set; }

        [Range(18, 100, ErrorMessage = "MaxAge phải từ 18 đến 100")]
        public int? MaxAge { get; set; }

        [Range(0, 1000, ErrorMessage = "Radius phải từ 0 đến 1000")]
        public int? Radius { get; set; }
    }
}