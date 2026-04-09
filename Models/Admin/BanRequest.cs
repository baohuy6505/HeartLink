using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.Admin;

public class BanRequest
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = null!;
}
