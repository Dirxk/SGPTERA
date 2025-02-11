using Microsoft.AspNetCore.Mvc;

namespace SGPTERA.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
