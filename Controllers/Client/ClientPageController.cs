using Microsoft.AspNetCore.Mvc;

namespace HeartLink.Controllers.Client;

public class ClientPageController : Controller
{
    [HttpGet("/")]
    public IActionResult Root() => Redirect("/login");

    [HttpGet("/login")]
    public IActionResult Login() => View();

    [HttpGet("/register")]
    public IActionResult Register() => View();

    [HttpGet("/change-password")]
    public IActionResult ChangePassword() => View();

    [HttpGet("/profile")]
    public IActionResult Profile() => View();

    [HttpGet("/profile/edit")]
    public IActionResult EditProfile() => View("EditProfile");

    [HttpGet("/profile/filter")]
    public IActionResult Filter() => View();

    [HttpGet("/discovery")]
    public IActionResult Discovery() => View();

    [HttpGet("/matches")]
    public IActionResult Matches() => View();

    [HttpGet("/messages/{matchId:int}")]
    public IActionResult Messages(int matchId)
    {
        ViewBag.MatchId = matchId;
        return View();
    }
}