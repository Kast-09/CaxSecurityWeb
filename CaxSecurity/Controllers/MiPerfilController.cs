using FireSharp.Interfaces;
using FireSharp;
using Microsoft.AspNetCore.Mvc;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using CaxSecurity.Models;
using Newtonsoft.Json;

namespace CaxSecurity.Controllers
{
    public class MiPerfilController : Controller
    {
        IFirebaseClient cliente;

        public MiPerfilController()
        {
            FirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "RoViu0tmUXIRx8EGnZglRYAzRX0F8qad5lv6Tnzo",
                BasePath = "https://caxsecurity-default-rtdb.firebaseio.com/"
            };

            cliente = new FirebaseClient(config);
        }

        public bool validarAdmin()
        {
            string userId = HttpContext.Session.GetString("_UserId");

            Usuario user = new Usuario();
            FirebaseResponse response = cliente.Get("Usuario/" + userId);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                user = JsonConvert.DeserializeObject<Usuario>(response.Body);
            }

            if (user.rol == "admin") return true;
            else
            {
                HttpContext.Session.Remove("_UserToken");
                HttpContext.Session.Remove("_UserId");
                return false;
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("_UserToken");

            if (token != null && validarAdmin())
            {
                string userId = HttpContext.Session.GetString("_UserId");

                FirebaseResponse response = cliente.Get("Usuario/" + userId);
                Usuario user = new Usuario();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    user = JsonConvert.DeserializeObject<Usuario>(response.Body);
                }

                user.id = userId;

                return View(user);
            }
            else
            {
                ModelState.AddModelError("AuthError", "No cuenta con los permisos");
                return RedirectToAction("Login", "Auth");
            }
        }
    }
}
