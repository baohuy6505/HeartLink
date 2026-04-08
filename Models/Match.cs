using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models;

[Table("MATCHES")]
public class Match
{
    [Key]
    public int MatchID { get; set; }

    [Required]
    public int LikeID { get; set; }

    [Required]
    public int User1ID { get; set; }

    [Required]
    public int User2ID { get; set; }

    public DateTime MatchedDate { get; set; } = DateTime.Now;

    public byte Status { get; set; } = 1; // 1: Connected, 2: Cancelled

    public Like Like { get; set; } = null!;
    public Account User1 { get; set; } = null!;
    public Account User2 { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}