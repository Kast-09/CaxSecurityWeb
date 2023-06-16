using CaxSecurity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using Newtonsoft.Json.Linq;
using FireSharp.Extensions;
using System.Xml.Linq;

namespace CaxSecurity.Controllers
{    
    public class HomeController : Controller
    {
        IFirebaseClient cliente;

        public HomeController()
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
            FirebaseResponse response = cliente.Get("Usuario/"+ userId);

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

        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("_UserToken");

            if (token != null && validarAdmin())
            {
                FirebaseResponse response = cliente.Get("Barrios");
                List<Barrio> barrios = new List<Barrio>();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    barrios = JsonConvert.DeserializeObject<List<Barrio>>(response.Body);
                }

                string date = DateTime.Today.ToString("dd_MM_yyyy");

                List<EstadoBarrio> estadoBarrios = new List<EstadoBarrio>();


                List<IdsAux> listaIds = new List<IdsAux>();
                response = cliente.Get("EstadoBarrios/" + date);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    listaIds = JsonConvert.DeserializeObject<List<IdsAux>>(response.Body);
                }

                int aux = 0;
                List<int> listaIdsTemp = new List<int>();
                if(listaIds != null)
                {
                    foreach (var elemento in listaIds)
                    {
                        if (elemento != null)
                        {
                            listaIdsTemp.Add(aux);
                        }
                        aux++;
                    }

                    foreach (int i in listaIdsTemp)
                    {
                        response = cliente.Get("EstadoBarrios/" + date + "/" + i);
                        EstadoBarrio estadoBarrio = new EstadoBarrio();

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            estadoBarrio = JsonConvert.DeserializeObject<EstadoBarrio>(response.Body);
                        }

                        estadoBarrios.Add(new EstadoBarrio()
                        {
                            Id = i.ToString(),
                            d1 = estadoBarrio.d1,
                            d2 = estadoBarrio.d2,
                            d3 = estadoBarrio.d3
                        });
                    }
                }

                List<MapaCalor> mapaCalors = new List<MapaCalor>();

                if (estadoBarrios != null)
                {
                    int cont = 0;
                    foreach (Barrio elemento in barrios)
                    {
                        if (cont > 0)
                        {
                            mapaCalors.Add(new MapaCalor()
                            {
                                Id = cont,
                                nombre = elemento.nombre,
                                estado = "",
                                d1 = 0,
                                d2 = 0,
                                d3 = 0
                            });
                        }
                        cont++;
                    }

                    foreach (EstadoBarrio estado in estadoBarrios)
                    {
                        int temp = Int32.Parse(estado.Id);
                        temp--;
                        mapaCalors[temp].d1 = estado.d1;
                        mapaCalors[temp].d2 = estado.d2;
                        mapaCalors[temp].d3 = estado.d3;
                    }
                }
                else
                {
                    int cont = 0;
                    foreach (Barrio elemento in barrios)
                    {
                        if (cont > 0)
                        {
                            mapaCalors.Add(new MapaCalor()
                            {
                                Id = cont,
                                nombre = elemento.nombre,
                                estado = "",
                                d1 = 0,
                                d2 = 0,
                                d3 = 0
                            });
                        }
                        cont++;
                    }
                }


                int cont2 = 0;
                foreach (MapaCalor mapaCalor in mapaCalors)
                {
                    int CantReportes = mapaCalor.d1 + mapaCalor.d2 + mapaCalor.d3;
                    if (CantReportes > 0 && CantReportes < 5)
                    {
                        mapaCalors[cont2].estado = "El barrio esta teniendo problemas.\n¡Se han reportado incidencias!";
                    }
                    else if (CantReportes > 5)
                    {
                        mapaCalors[cont2].estado = "El barrio esta teniendo muchos problemas.\n¡Se han reportado demasiadas incidencias!";
                    }
                    else
                    {
                        mapaCalors[cont2].estado = "El barrio esta tranquilo.\n¡No se han reportado incidencias por ahora!";
                    }
                    cont2++;
                }
                return View(mapaCalors);
            }
            else
            {
                ModelState.AddModelError("AuthError", "No cuenta con los permisos");
                return RedirectToAction("Login","Auth");
            }            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}