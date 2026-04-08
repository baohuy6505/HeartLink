using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models;

[Table("PROFILE")]
public class Profile
{
    [Key]
    public int ProfileID { get; set; }

    [Required]
    public int AccountID { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    public DateTime BirthDate { get; set; }

    [Required, MaxLength(10)]
    public string Gender { get; set; } = null!;

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    [MaxLength(255)]
    public string? Avatar { get; set; }

    [MaxLength(10)]
    public string? TargetGender { get; set; }

    public int MinAge { get; set; } = 18;

    public int MaxAge { get; set; } = 99;

    public double Radius { get; set; } = 50;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public Account Account { get; set; } = null!;

    public ICollection<ProfileInterest> Interests { get; set; } = new List<ProfileInterest>();
}