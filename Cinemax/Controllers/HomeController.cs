using Cinemax.Models;
using Cinemax.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.Controllers
{
    public class HomeController : Controller
    {
        private CinemaxEntities _dbContext;

        public HomeController()
        {
            _dbContext = new CinemaxEntities();
        }


        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }


        public ActionResult Login(bool? error)
        {

            if (error == false)
            {
                ViewData["ErrorMessage"] = null;
            }
            else if (Session["usuarioId"] != null)
            {
                return RedirectToDashboard();
            }

            return View();
        }

        [HttpPost]
        public ActionResult Login(string txtUsuario, string txtClave)
        {
            var usuario = _dbContext.Usuario
                  .Include("Rol")
                  .FirstOrDefault(u => u.USU_Email == txtUsuario
                                   && u.USU_Password == txtClave
                                   && (u.Rol.ROL_Nombre == "Cliente" || u.Rol.ROL_Nombre == "Empleado"));

            if (usuario != null)
            {

                Session["usuarioId"] = usuario.ID_Usuario;
                Session["TipoUsuario"] = usuario.Rol.ROL_Nombre;
                Session["Nombre"] = usuario.USU_Nombre;

                return RedirectToDashboard();
            }

            ViewData["ErrorMessage"] = "Error, usuario inválido";
            return View();
        }


        [Autenticacion]
        public ActionResult Dashboard()
        {
            if (Session["TipoUsuario"] as string != "Empleado")
            {
                return RedirectToAction("Login");
            }

            ViewBag.Nombre = Session["Nombre"];
            ViewBag.TipoUsuario = Session["TipoUsuario"];
            return View();
        }


        [Autenticacion]
        public ActionResult DashboardCliente()
        {
            if (Session["TipoUsuario"] as string != "Cliente")
            {
                return RedirectToAction("Login");
            }

            ViewBag.Nombre = Session["Nombre"];
            ViewBag.TipoUsuario = Session["TipoUsuario"];
            return View();
        }


        public ActionResult Registro()
        {
            return View();
        }


        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();


            if (Request.Cookies["CinemaxSession"] != null)
            {
                var cookie = new HttpCookie("CinemaxSession")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Login", new { error = false });
        }

        // Método redirección automática
        private ActionResult RedirectToDashboard()
        {
            var tipoUsuario = Session["TipoUsuario"] as string;
            return tipoUsuario == "Cliente"
                ? RedirectToAction("DashboardCliente")
                : RedirectToAction("Dashboard");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _dbContext != null)
            {
                _dbContext.Dispose();
                _dbContext = null;
            }
            base.Dispose(disposing);
        }
    }
}