using FireSharp.Interfaces;
using FireSharp;
using Microsoft.AspNetCore.Mvc;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using CaxSecurity.Models;
using System.Xml.Linq;

namespace CaxSecurity.Controllers
{
    public class ReportesController : Controller
    {
        IFirebaseClient cliente;

        public ReportesController()
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
            string date = DateTime.Today.ToString("dd_MM_yyyy");
            Dictionary<string, ReportesDia> lista = new Dictionary<string, ReportesDia>();
            FirebaseResponse response = cliente.Get("DataReportes/"+date);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                lista = JsonConvert.DeserializeObject<Dictionary<string, ReportesDia>>(response.Body);
            }

            List<ReportesDia> listaContacto = new List<ReportesDia>();

            foreach (KeyValuePair<string, ReportesDia> elemento in lista)
            {
                listaContacto.Add(new ReportesDia()
                {
                    Id = elemento.Key.ToString(),
                    idReporte = elemento.Value.idReporte,
                    idUsuario = elemento.Value.idUsuario
                });
            }


            List<Reportes> listaReportesVista = new List<Reportes>();
            foreach (ReportesDia aux in listaContacto)
            {
                Dictionary<Reportes> listaReportes = new Dictionary<Reportes>();
                Reportes reporteAux = new Reportes();
                response = cliente.Get("Reportes/" + aux.idUsuario + "/" + aux.idReporte);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    reporteAux = JsonConvert.DeserializeObject<Reportes>(response.Body);
                }

                listaReportesVista.Add(new Reportes()
                {
                    descripcionReporte = reporteAux.descripcionReporte,
                    nombreBarrio = reporteAux.nombreBarrio,
                    tipoReporte = reporteAux.tipoReporte
                });
            }

            return View(listaReportesVista);
        }
    }
}
