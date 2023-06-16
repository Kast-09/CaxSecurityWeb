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
using System.Runtime.Intrinsics.X86;

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
                Dictionary<string, ReportesDia> lista = new Dictionary<string, ReportesDia>();
                FirebaseResponse response = cliente.Get("IndicesReportes/Enviados");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    lista = JsonConvert.DeserializeObject<Dictionary<string, ReportesDia>>(response.Body);
                }


                List<ReportesDia> listaContacto = new List<ReportesDia>();
                List<ReportesIntermedia> listaReportesVista = new List<ReportesIntermedia>();

                if (lista != null)
                {
                    foreach (KeyValuePair<string, ReportesDia> elemento in lista)
                    {
                        listaContacto.Add(new ReportesDia()
                        {
                            Id = elemento.Key.ToString(),
                            idReporte = elemento.Value.idReporte,
                            idUsuario = elemento.Value.idUsuario
                        });
                    }

                    foreach (ReportesDia aux in listaContacto)
                    {
                        Dictionary<ReportesIntermedia> listaReportes = new Dictionary<ReportesIntermedia>();
                        ReportesIntermedia reporteAux = new ReportesIntermedia();
                        response = cliente.Get("Reportes/" + aux.idUsuario + "/" + aux.idReporte);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            reporteAux = JsonConvert.DeserializeObject<ReportesIntermedia>(response.Body);
                        }

                        listaReportesVista.Add(new ReportesIntermedia()
                        {
                            descripcionReporte = reporteAux.descripcionReporte,
                            direccion = reporteAux.direccion,
                            estado = reporteAux.estado,
                            fecha = reporteAux.fecha,
                            linkMultimedia = reporteAux.linkMultimedia,
                            nombreBarrio = reporteAux.nombreBarrio,
                            referencia = reporteAux.referencia,
                            tipoReporte = reporteAux.tipoReporte,
                            idReporte = aux.idReporte,
                            idUsuario = aux.idUsuario
                        });
                    }
                }

                return View(listaReportesVista);
            }
            else
            {
                ModelState.AddModelError("AuthError", "No cuenta con los permisos");
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost]
        public string verIds(string idUsuario, string idReporte)
        {
            string temp = "";

            FirebaseResponse response = cliente.Get("Reportes/" + idUsuario + "/" + idReporte);
            ReportesIntermedia reporteAux = new ReportesIntermedia();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                reporteAux = JsonConvert.DeserializeObject<ReportesIntermedia>(response.Body);
            }

            response = cliente.Get("Usuario/" + idUsuario);
            Usuario usuario = new Usuario();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                usuario = JsonConvert.DeserializeObject<Usuario>(response.Body);
            }


            temp += "<div class=\"modal-body\" id=\"aux\">\r\n" +
                "<form action=\"/Reportes/actualizarEstado\" method=\"post\">" +
                "<input type=\"hidden\" name=\"Id\" value=\""+ reporteAux.Id +"\" />" +
                "<input type=\"hidden\" name=\"idUsuario\" value=\"" + idUsuario + "\" />" +
                "<input type=\"hidden\" name=\"idReporte\" value=\"" + idReporte + "\" />" +
                "<input type=\"hidden\" name=\"descripcionReporte\" value=\"" + reporteAux.descripcionReporte + "\" />" +
                "<input type=\"hidden\" name=\"direccion\" value=\"" + reporteAux.direccion + "\" />" +
                "<input type=\"hidden\" name=\"fecha\" value=\"" + reporteAux.fecha + "\" />" +
                "<input type=\"hidden\" name=\"ip\" value=\""+ reporteAux.ip +"\" />" +
                "<input type=\"hidden\" name=\"linkMultimedia\" value=\"" + reporteAux.linkMultimedia + "\" />" +
                "<input type=\"hidden\" name=\"nombreBarrio\" value=\"" + reporteAux.nombreBarrio + "\" />" +
                "<input type=\"hidden\" name=\"referencia\" value=\"" + reporteAux.referencia + "\" />" +
                "<input type=\"hidden\" name=\"tipoReporte\" value=\"" + reporteAux.tipoReporte + "\" />" +
                "<input type=\"hidden\" name=\"tipoMultimedia\" value=\"" + reporteAux.tipoMultimedia + "\" />" +
                "<p><b>Usuario reportante:</b> " + usuario.nombre + "</p>\r\n" +
                "<p><b>DNI:</b> " + usuario.dni + " </p>\r\n    " +
                "<p><b>Número de celular:</b> " + usuario.telefono + "</p>\r\n    " +
                "<p><b>Tipo reporte:</b> "+ reporteAux.tipoReporte +"</p>\r\n    " +
                "<p><b>Descripción:</b> " + reporteAux.descripcionReporte + " </p>\r\n    " +
                "<p><b>Nombre Barrio:</b> " + reporteAux.nombreBarrio + " </p>\r\n    " +
                "<p><b>Direccción:</b> " + reporteAux.direccion + "</p>\r\n    " +
                "<p><b>Referencia:</b> " + reporteAux.referencia + "</p>\r\n    " +
                "<select class=\"form-select form-select-sm\" aria-label=\"form-select-sm\" name=\"estado\">\r\n" +
                "<option selected>-- Seleccione estado --</option>\r\n      ";

            switch (reporteAux.estado)
            {
                case "ENVIADO":
                    temp += "<option value=\"1\" selected>ENVIADO</option>\r\n      " +
                "<option value=\"2\">SE TOMO CONOCIMIENTO</option>\r\n      " +
                "<option value=\"3\">CONSTITUYENDOSE AL LUGAR DE LOS HECHOS</option>\r\n      " +
                "<option value=\"4\">RESUELTO</option>\r\n      " +
                "<option value=\"5\">CANCELADO</option>\r\n"; break;
                case "SE TOMO CONOCIMIENTO":
                    temp += "<option value=\"1\">ENVIADO</option>\r\n      " +
                "<option value=\"2\" selected>SE TOMO CONOCIMIENTO</option>\r\n      " +
                "<option value=\"3\">CONSTITUYENDOSE AL LUGAR DE LOS HECHOS</option>\r\n      " +
                "<option value=\"4\">RESUELTO</option>\r\n      " +
                "<option value=\"5\">CANCELADO</option>\r\n"; break;
                case "CONSTITUYENDOSE AL LUGAR DE LOS HECHOS":
                    temp += "<option value=\"1\">ENVIADO</option>\r\n      " +
                "<option value=\"2\">SE TOMO CONOCIMIENTO</option>\r\n      " +
                "<option value=\"3\" selected>CONSTITUYENDOSE AL LUGAR DE LOS HECHOS</option>\r\n      " +
                "<option value=\"4\">RESUELTO</option>\r\n      " +
                "<option value=\"5\">CANCELADO</option>\r\n"; break;
                case "RESUELTO":
                    temp += "<option value=\"1\">ENVIADO</option>\r\n      " +
                "<option value=\"2\">SE TOMO CONOCIMIENTO</option>\r\n      " +
                "<option value=\"3\">CONSTITUYENDOSE AL LUGAR DE LOS HECHOS</option>\r\n      " +
                "<option value=\"4\" selected>RESUELTO</option>\r\n      " +
                "<option value=\"5\">CANCELADO</option>\r\n"; break;
                case "CANCELADO":
                    temp += "<option value=\"1\">ENVIADO</option>\r\n      " +
                "<option value=\"2\">SE TOMO CONOCIMIENTO</option>\r\n      " +
                "<option value=\"3\">CONSTITUYENDOSE AL LUGAR DE LOS HECHOS</option>\r\n      " +
                "<option value=\"4\">RESUELTO</option>\r\n      " +
                "<option value=\"5\" selected>CANCELADO</option>\r\n"; break;
            }

            temp += "</select><br />";

            switch (reporteAux.tipoMultimedia)
            {
                case "vacio": break;
                case "imagen":
                    temp += "<img src=\""+ reporteAux.linkMultimedia +"\" alt=\"Imagen reporte\">"; break;
                case "video":
                    temp += "<video controls style:\"text-align:center; align-items:center;\"  width=\"426\" height=\"240\">\r\n" +
                    "<source src=\"" + reporteAux.linkMultimedia + "\" type=\"video/mp4\">\r\n" +
                    "<p>Multimedia</p>\r\n" +
                    "</video>";break;
            }          

            temp += "<button type=\"submit\" class=\"btn btn-success\">" +
                "<i class=\"bi bi-check-circle\"></i> Actualizar Estado </i></button>\r\n" +
                "<a class=\"btn btn-secondary\" data-dismiss=\"modal\" onclick=\"$('#modalVerReportes').modal('hide');\">" +
                "<i class=\"bi bi-box-arrow-down-left\"> Regresar</i></a>" +
                "</form>" +
                "</div>\r\n" +
                "<br /><div class=\"modal-footer\" style=\"align-items:end\">\r\n" +
                "<p id=\"fecha\"> Reporte realizado: "+reporteAux.fecha+"</p>\r\n            " +
                "</div>";

            return temp;
        }

        [HttpPost]
        public IActionResult actualizarEstado(ReportesIntermedia reportes)
        {
            Reportes reporte = new Reportes();
            reporte.Id = "";
            reporte.descripcionReporte = reportes.descripcionReporte;
            reporte.direccion = reportes.direccion;
            reporte.estado = reportes.estado;
            switch (reportes.estado)
            {
                case "1": reporte.estado = "ENVIADO"; break;
                case "2": reporte.estado = "SE TOMO CONOCIMIENTO"; break;
                case "3": reporte.estado = "CONSTITUYENDOSE AL LUGAR DE LOS HECHOS"; break;
                case "4": reporte.estado = "RESUELTO"; break;
                case "5": reporte.estado = "CANCELADO"; break;
            }
            reporte.fecha = reportes.fecha;
            reporte.ip = reportes.ip;
            reporte.linkMultimedia = reportes.linkMultimedia;
            reporte.nombreBarrio = reportes.nombreBarrio;
            reporte.referencia = reportes.referencia;
            reporte.tipoReporte = reportes.tipoReporte;
            reporte.tipoMultimedia = reportes.tipoMultimedia;

            FirebaseResponse response = cliente.Update("Reportes/" + reportes.idUsuario + "/" + reportes.idReporte, reporte);

            return RedirectToAction("Index");
        }
    }
}
