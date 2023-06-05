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

        public IActionResult Index()
        {
            Dictionary<float, Barrio> lista2 = new Dictionary<float, Barrio>();
            FirebaseResponse response = cliente.Get("Barrios");
            List<Barrio> barrios = new List<Barrio>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                barrios = JsonConvert.DeserializeObject<List<Barrio>>(response.Body);
            }

            string date = DateTime.Today.ToString("dd_MM_yyyy");

            Dictionary<float, EstadoBarrio> lista = new Dictionary<float, EstadoBarrio>();
            FirebaseResponse response2 = cliente.Get("EstadoBarrios/" + date);
            List<EstadoBarrio> estadoBarrios = new List<EstadoBarrio>();

            if (response2.StatusCode == System.Net.HttpStatusCode.OK)
            {
                lista = JsonConvert.DeserializeObject<Dictionary<float, EstadoBarrio>>(response2.Body);
            }

            
            List<MapaCalor> mapaCalors = new List<MapaCalor>();
            List<Barrio> listaBarrios = new List<Barrio>();

            if(lista != null)
            {
                foreach (KeyValuePair<float, EstadoBarrio> elemento in lista)
                {
                    estadoBarrios.Add(new EstadoBarrio()
                    {
                        Id = elemento.Key.ToString(),
                        d1 = elemento.Value.d1,
                        d2 = elemento.Value.d2,
                        d3 = elemento.Value.d3
                    });
                }

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}