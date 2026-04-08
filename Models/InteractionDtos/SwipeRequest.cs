using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.InteractionDtos;

public class SwipeRequest
{
    [Required]
    public int ReceiverID { get; set; }

    [Required, RegularExpression("Like|Pass")]
    public string Type { get; set; } = null!;
}