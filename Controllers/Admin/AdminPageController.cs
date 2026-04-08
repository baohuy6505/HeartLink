using Microsoft.AspNetCore.Mvc;

namespace HeartLink.Controllers.Admin;

public class AdminPageController : Controller
{
    [HttpGet("/admin")]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet("/admin/users")]
    public IActionResult Users()
    {
        return View();
    }
}