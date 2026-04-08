using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models;

[Table("ACCOUNT")]
public class Account
{
    [Key]
    public int AccountID { get; set; }

    [Required, MaxLength(100), EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MaxLength(255)]
    public string Password { get; set; } = null!;

    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public bool Status { get; set; } = true;

    [Required, MaxLength(20)]
    public string UserRole { get; set; } = "User";

    public Profile? Profile { get; set; }

    public ICollection<Like> SentLikes { get; set; } = new List<Like>();
    public ICollection<Like> ReceivedLikes { get; set; } = new List<Like>();
    public ICollection<Match> MatchesAsUser1 { get; set; } = new List<Match>();
    public ICollection<Match> MatchesAsUser2 { get; set; } = new List<Match>();
    public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
}