using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class UiAccountController : Controller
    {
        public IActionResult Register()
        {
            return View();
        }
    }
}
