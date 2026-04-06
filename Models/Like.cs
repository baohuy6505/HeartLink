using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models
{
    public class Like
    {
        [Key]
        public long LikeID { get; set; }

        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Type { get; set; } = "Like";
        public DateTime CreatedAt { get; set; }

        [ForeignKey("SenderID")]
        public Account? Sender { get; set; }

        [ForeignKey("ReceiverID")]
        public Account? Receiver { get; set; }
    }
}