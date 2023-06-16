using Microsoft.AspNetCore.Mvc;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using CaxSecurity.Models;
using System.Runtime.Intrinsics.X86;

namespace CaxSecurity.Controllers
{
    public class UsuariosController : Controller
    {
        IFirebaseClient cliente;

        public UsuariosController()
        {
            FirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "RoViu0tmUXIRx8EGnZglRYAzRX0F8qad5lv6Tnzo",
                BasePath = "https://caxsecurity-default-rtdb.firebaseio.com/"
            };

            cliente = new FirebaseClient(config);
        }

        [HttpGet]
        public IActionResult Index()
        {
            Dictionary<string, IdsAux> lista = new Dictionary<string, IdsAux>();
            FirebaseResponse response = cliente.Get("Usuario");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                lista = JsonConvert.DeserializeObject<Dictionary<string, IdsAux>>(response.Body);
            }

            List<string> listaUsuarios = new List<string>();

            foreach (KeyValuePair<string, IdsAux> elemento in lista)
            {
                listaUsuarios.Add(elemento.Key);
            }

            List<string> listaIdsReportes = new List<string>();

            List<Usuario> usuarios = new List<Usuario>();

            foreach(string aux in listaUsuarios)
            {
                response = cliente.Get("Usuario/"+aux);
                Usuario usuario = new Usuario();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    usuario = JsonConvert.DeserializeObject<Usuario>(response.Body);
                }

                usuario.id = aux; 
                usuarios.Add(usuario);
            }

            return View(usuarios);
        }

        [HttpGet]
        public IActionResult VerUsuario(string id)
        {
            FirebaseResponse response = cliente.Get("Usuario/" + id);
            Usuario usuario = new Usuario();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                usuario = JsonConvert.DeserializeObject<Usuario>(response.Body);
            }

            usuario.id = id;

            response = cliente.Get("Reportes/" + id);
            Dictionary<string, Reportes> lista = new Dictionary<string, Reportes>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                lista = JsonConvert.DeserializeObject<Dictionary<string, Reportes>>(response.Body);
            }

            List<ReportesIntermedia> reportes = new List<ReportesIntermedia>();

            foreach (KeyValuePair<string, Reportes> elemento in lista)
            {
                reportes.Add(new ReportesIntermedia()
                {
                    idUsuario = id,
                    idReporte = elemento.Key,
                    descripcionReporte = elemento.Value.descripcionReporte,
                    direccion = elemento.Value.direccion,
                    estado = elemento.Value.estado,
                    fecha = elemento.Value.fecha,
                    linkMultimedia = elemento.Value.linkMultimedia,
                    nombreBarrio = elemento.Value.nombreBarrio,
                    referencia = elemento.Value.referencia,
                    tipoReporte = elemento.Value.tipoReporte
                });
            }

            ViewBag.reportes = reportes;

            return View(usuario);
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
                "<p><b>Usuario reportante:</b> " + usuario.nombre + "</p>\r\n" +
                "<p><b>DNI:</b> " + usuario.dni + " </p>\r\n    " +
                "<p><b>Número de celular:</b> " + usuario.telefono + "</p>\r\n    " +
                "<p><b>Tipo reporte:</b> " + reporteAux.tipoReporte + "</p>\r\n    " +
                "<p><b>Descripción:</b> " + reporteAux.descripcionReporte + " </p>\r\n    " +
                "<p><b>Nombre Barrio:</b> " + reporteAux.nombreBarrio + " </p>\r\n    " +
                "<p><b>Direccción:</b> " + reporteAux.direccion + "</p>\r\n    " +
                "<p><b>Referencia:</b> " + reporteAux.referencia + "</p>\r\n    " +
                "<p><b>Estado:</b> " + reporteAux.estado + "</p>\r\n    ";

            switch (reporteAux.tipoMultimedia)
            {
                case "vacio": break;
                case "imagen":
                    temp += "<img src=\"" + reporteAux.linkMultimedia + "\" alt=\"Imagen reporte\">"; break;
                case "video":
                    temp += "<video controls style:\"text-align:center; align-items:center;\"  width=\"426\" height=\"240\">\r\n" +
                    "<source src=\"" + reporteAux.linkMultimedia + "\" type=\"video/mp4\">\r\n" +
                    "<p>Multimedia</p>\r\n" +
                    "</video>"; break;
            }

            temp += "<a class=\"btn btn-secondary\" data-dismiss=\"modal\" onclick=\"$('#modalVerReportesUsuario').modal('hide');\">" +
                "<i class=\"bi bi-box-arrow-down-left\"> Regresar</i></a>" +
                "</div>\r\n" +
                "<br /><div class=\"modal-footer\" style=\"align-items:end\">\r\n" +
                "<p id=\"fecha\"> Reporte realizado: " + reporteAux.fecha + "</p>\r\n            " +
                "</div>";

            return temp;
        }
    }
}
