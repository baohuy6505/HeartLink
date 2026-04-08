using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.InteractionDtos;

public class SendMessageRequest
{
    [Required]
    public int MatchID { get; set; }

    [Required]
    public string Content { get; set; } = null!;
}