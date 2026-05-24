using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class AccountControllerTesting : Controller
    {
        public IActionResult Register()
        {
            return View();
        }
    }
}
