using Microsoft.AspNetCore.Mvc;

namespace CaxSecurity.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
