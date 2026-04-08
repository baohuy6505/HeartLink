using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models;

[Table("LIKES")]
public class Like
{
    [Key]
    public int LikeID { get; set; }

    [Required]
    public int SenderID { get; set; }

    [Required]
    public int ReceiverID { get; set; }

    [Required, MaxLength(10)]
    public string Type { get; set; } = null!; // Like / Pass

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public Account Sender { get; set; } = null!;
    public Account Receiver { get; set; } = null!;
}