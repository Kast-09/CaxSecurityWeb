using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaxSecurity.Controllers
{
    public class AuthController : Controller
    {
        private static string ApiKey = "AIzaSyD0wFVmCaWbAYYfPXfPKv8n2rRx4PPsDlU";
        //private static string Bucket = "asp-mvc-with-android.appspot.com";

        //GET: Account
        /*public ActionResult SignUp()
        {
            return View();
        }*/

        /*[HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SignUp(SignUpModel model)
        {

        }*/

        /*[AllowAnonymous]
        [HttpGet]*/
        public IActionResult Login(string returnUrl)
        {
            /*try
            {
                //verificacion
                if (User.Identity.IsAuthenticated)
                {
                    //return this.RedirectToLocal(returnUrl);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }*/
            return View();
        }
    }
}
