using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models
{
    public class Account
    {
        [Key]
        public int AccountID { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = "Active";
        public string Role { get; set; } = "User";
        public DateTime? LastLoginAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Profile? Profile { get; set; }

        public ICollection<Like> SentLikes { get; set; } = new List<Like>();
        public ICollection<Like> ReceivedLikes { get; set; } = new List<Like>();
        public ICollection<Match> MatchesAsUser1 { get; set; } = new List<Match>();
        public ICollection<Match> MatchesAsUser2 { get; set; } = new List<Match>();
        public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
    }
}