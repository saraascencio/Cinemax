using Cinemax.Models;
using Cinemax.Servicios;
using Cinemax.ViewModels;
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

        public ActionResult HistorialReservas()
        {
            
            var datosReservas = (from r in _dbContext.Reserva
                             join f in _dbContext.Funcion on r.ID_Funcion equals f.ID_Funcion
                             join p in _dbContext.Pelicula on f.ID_Pelicula equals p.ID_Pelicula
                             join s in _dbContext.Sala on f.ID_Sala equals s.ID_Sala
                             join er in _dbContext.EstadoReserva on r.ID_REstado equals er.ID_Estadoreserva
                             join b in _dbContext.Boleto on r.ID_Reserva equals b.ID_Reserva into boletos
                             from b in boletos.DefaultIfEmpty()
                             join a in _dbContext.Asiento on (b == null ? null : (int?)b.ID_Asiento) equals a.ID_Asiento into asientos
                             from a in asientos.DefaultIfEmpty()
                             // Utilizamos esta operación de agrupamiento de objetos anónimos
                             // Con el fin de faciltar las operaciones por grupo bajo un id reserva
                             group new { r, f, p, s, er, a } by r.ID_Reserva into g
                             select new
                             {
                                 ID_Reserva = g.Key,
                                 FuncionFecha = g.Select(x => x.f.FUN_Fechahora).FirstOrDefault(),
                                 PeliculaNombre = g.Select(x => x.p.PEL_Titulo).FirstOrDefault(),
                                 Precio = g.Select(x => x.f.FUN_Precio).FirstOrDefault(),
                                 SalaNombre = g.Select(x => x.s.SAL_Nombre).FirstOrDefault(),
                                 AsientosData = g.Where(x => x.a != null)
                                               .Select(x => new {
                                                   Fila = x.a.ASI_Fila,
                                                   Numero = x.a.ASI_Numero
                                               }),
                                 QR = g.Select(x => x.r.RES_QR).FirstOrDefault(),
                                 Estado = g.Select(x => x.er.ESR_Estado).FirstOrDefault()
                             }).AsEnumerable(); 

            // Se optó por ViewModel para garantizar mejor funcionamiento
            // por la unión de varias tablas que se da
            var listadoFinal = datosReservas.Select(x => new ReservaViewModel
            {
                ID_Reserva = x.ID_Reserva,
                FuncionFecha = x.FuncionFecha,
                PeliculaNombre = x.PeliculaNombre,
                Precio = x.Precio,
                SalaNombre = x.SalaNombre,
                Asientos = string.Join(", ", x.AsientosData.Select(a => $"{a.Fila}{a.Numero}").Distinct()),
                QR = x.QR,
                Estado = x.Estado
            }).ToList();

            return View(listadoFinal);
        }

    }
}