using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models;

[Table("MESSAGE")]
public class Message
{
    [Key]
    public long MessageID { get; set; }

    [Required]
    public int MatchID { get; set; }

    [Required]
    public int SenderID { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    public DateTime SentTime { get; set; } = DateTime.Now;

    public bool IsRead { get; set; } = false;

    public Match Match { get; set; } = null!;
    public Account Sender { get; set; } = null!;
}