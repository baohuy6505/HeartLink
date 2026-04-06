using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models
{
    public class Message
    {
        [Key]
        public long MessageID { get; set; }

        public long MatchID { get; set; }
        public int SenderID { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentTime { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadTime { get; set; }
        public string Status { get; set; } = "Sent";

        [ForeignKey("MatchID")]
        public Match? Match { get; set; }

        [ForeignKey("SenderID")]
        public Account? Sender { get; set; }
    }
}