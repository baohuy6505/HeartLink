using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models
{
    public class Profile
    {
        [Key]
        public int ProfileID { get; set; }

        public int AccountID { get; set; }
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Avatar { get; set; }
        public string? Interests { get; set; }
        public string? TargetGender { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int? Radius { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("AccountID")]
        public Account? Account { get; set; }
    }
}