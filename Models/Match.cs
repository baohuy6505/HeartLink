using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeartLink.Models
{
    public class Match
    {
        [Key]
        public long MatchID { get; set; }

        public int User1ID { get; set; }
        public int User2ID { get; set; }
        public DateTime MatchedDate { get; set; }
        public string Status { get; set; } = "Active";
        public long? CreatedByLikeA { get; set; }
        public long? CreatedByLikeB { get; set; }

        [ForeignKey("User1ID")]
        public Account? User1 { get; set; }

        [ForeignKey("User2ID")]
        public Account? User2 { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}