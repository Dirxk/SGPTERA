using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
