using Cinemax.Models;
using Cinemax.Servicios;
using Cinemax.ViewModels;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;

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
        public ActionResult DashboardCliente()
        {
            if (Session["TipoUsuario"] as string != "Cliente")
            {
                return RedirectToAction("Login");
            }
            ViewBag.idUsuario = Session["usuarioId"];
            ViewBag.Nombre = Session["Nombre"];
            ViewBag.TipoUsuario = Session["TipoUsuario"];
            return View();
        }

        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registro(string Name, string Email, string Password)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ViewBag.ErrorRegistro = "Todos los campos son obligatorios.";
                return View();
            }

            var usuarioExistente = _dbContext.Usuario.FirstOrDefault(u => u.USU_Email == Email);
            if (usuarioExistente != null)
            {
                ViewBag.ErrorRegistro = "Ya existe una cuenta con este correo electrónico.";
                return View();
            }
            if (Password.Length < 8)
            {
                ViewBag.ErrorRegistro = "La contraseña debe tener al menos 8 caracteres.";
                return View();
            }

            var nuevoUsuario = new Usuario
            {
                USU_Nombre = Name,
                USU_Email = Email,
                USU_Password = HashPassword(Password), 
                USU_FRegistro = DateTime.Now,
                ID_Rol = 3 // Rol Cliente
            };

            _dbContext.Usuario.Add(nuevoUsuario);
            _dbContext.SaveChanges();

            TempData["RegistroExitoso"] = true;
            return RedirectToAction("Login");
        }


        [Autenticacion]
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
                : RedirectToAction("Dashboard", "EmpleadoMetricas");
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



        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

    }
}