using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models;

[Table("PROFILE_INTERESTS")]
public class ProfileInterest
{
    [Required]
    public int ProfileID { get; set; }

    [Required, MaxLength(50)]
    public string InterestName { get; set; } = null!;

    public Profile Profile { get; set; } = null!;
}