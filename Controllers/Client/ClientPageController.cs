using Microsoft.AspNetCore.Mvc;

namespace HeartLink.Controllers.Client;

public class ClientPageController : Controller
{
    [HttpGet("/")]
    public IActionResult Root()
    {
        return Redirect("/login");
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet("/change-password")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpGet("/profile")]
    public IActionResult Profile()
    {
        return View();
    }

    [HttpGet("/profile/edit")]
    public IActionResult EditProfile()
    {
        return View();
    }

    [HttpGet("/profile/filter")]
    public IActionResult Filter()
    {
        return View();
    }

    [HttpGet("/discovery")]
    public IActionResult Discovery()
    {
        return View();
    }

    [HttpGet("/matches")]
    public IActionResult Matches()
    {
        return View();
    }

    [HttpGet("/messages/{matchId:int}")]
    public IActionResult Messages(int matchId)
    {
        ViewBag.MatchId = matchId;
        return View();
    }
}