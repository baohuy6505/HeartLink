using System.ComponentModel.DataAnnotations;

namespace HeartLink.Models.ProfileDtos;

public class SetFilterRequest
{
    [MaxLength(10)]
    public string TargetGender { get; set; } = "Tất cả";

    [Range(18, 99)]
    public int MinAge { get; set; } = 18;

    [Range(18, 99)]
    public int MaxAge { get; set; } = 99;

    [Range(1, 500)]
    public double Radius { get; set; } = 50;
}