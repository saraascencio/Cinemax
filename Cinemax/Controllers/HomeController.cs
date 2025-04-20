using Cinemax.Models;
using Cinemax.Servicios;
using Cinemax.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                Asientos = string.Join(",", x.AsientosData.Select(a => $"{a.Fila}{a.Numero}").Distinct()),
                QR = x.QR,
                Estado = x.Estado
            }).ToList();

            return View(listadoFinal);
        }


        public ActionResult Detalles(int id)
        {
            var consultaDetalle = (from r in _dbContext.Reserva
                         join f in _dbContext.Funcion on r.ID_Funcion equals f.ID_Funcion
                         join p in _dbContext.Pelicula on f.ID_Pelicula equals p.ID_Pelicula
                         join s in _dbContext.Sala on f.ID_Sala equals s.ID_Sala
                         join er in _dbContext.EstadoReserva on r.ID_REstado equals er.ID_Estadoreserva
                         join b in _dbContext.Boleto on r.ID_Reserva equals b.ID_Reserva into boletos
                         from b in boletos.DefaultIfEmpty()
                         join a in _dbContext.Asiento on (b == null ? null : (int?)b.ID_Asiento) equals a.ID_Asiento into asientos
                         from a in asientos.DefaultIfEmpty()
                         where r.ID_Reserva == id
                         select new
                         {
                             r.ID_Reserva,
                             p.PEL_Titulo,
                             f.FUN_Fechahora,
                             f.FUN_Precio,
                             s.SAL_Nombre,
                             AsientoFila = a != null ? a.ASI_Fila : null,
                             AsientoNumero = a != null ? (int?)a.ASI_Numero : null,
                             r.RES_QR,
                             er.ESR_Estado
                         }).ToList(); 

            var reservaAgrupada = consultaDetalle
                .GroupBy(x => x.ID_Reserva)
                .Select(g => new ReservaViewModel
                {
                    ID_Reserva = g.Key,
                    PeliculaNombre = g.First().PEL_Titulo,
                    FuncionFecha = g.First().FUN_Fechahora,
                    Precio = g.First().FUN_Precio,
                    SalaNombre = g.First().SAL_Nombre,
                    Asientos = string.Join(",", g
                        .Where(x => x.AsientoFila != null && x.AsientoNumero != null)
                        .Select(x => $"{x.AsientoFila}{x.AsientoNumero}")
                        .Distinct()),
                    QR = g.First().RES_QR,
                    Estado = g.First().ESR_Estado
                }).FirstOrDefault();

            if (reservaAgrupada == null)
            {
                return HttpNotFound();
            }

            return View(reservaAgrupada);
        }

        public ActionResult Editar(int id, int? idPeliculaSeleccionada, int? idFuncionSeleccionada,
             string asientosSeleccionados = null)
        {

            var reservaActual = _dbContext.Reserva
                .Include("Funcion")
                .Include("Funcion.Pelicula")
                .Include("Funcion.Sala")
                .Include("Boleto")
                .Include("Boleto.Asiento")
                .Include("EstadoReserva")
                .FirstOrDefault(r => r.ID_Reserva == id);

            if (reservaActual == null)
            {
                return HttpNotFound();
            }


            int peliculaId = idPeliculaSeleccionada ?? reservaActual.Funcion?.ID_Pelicula ?? 0;
            int funcionId = idFuncionSeleccionada ?? reservaActual.ID_Funcion ?? 0;


            var funcionesData = _dbContext.Funcion
                .Where(f => f.ID_Pelicula == peliculaId)
                .ToList();

            var peliculasData = _dbContext.Pelicula.ToList();
            var estadosData = _dbContext.EstadoReserva.ToList();


            
            var asientosActuales = new List<string>();

            // Caso especial cuando se indica explícitamente vacío
            if (asientosSeleccionados == "Vacio")
            {
                asientosActuales = new List<string>();
            }
            // Caso normal con asientos seleccionados
            else if (!string.IsNullOrEmpty(asientosSeleccionados))
            {
                asientosActuales = asientosSeleccionados.Split(',').ToList();
            }
            // Caso inicial (cargar asientos originales)
            else
            {
                asientosActuales = reservaActual.Boleto?
                    .Where(b => b.Asiento != null)
                    .Select(b => $"{b.Asiento.ASI_Fila}{b.Asiento.ASI_Numero}")
                    .ToList() ?? new List<string>();
            }

            var funciones = funcionesData
                .Select(f => new SelectListItem
                {
                    Value = f.ID_Funcion.ToString(),
                    Text = f.FUN_Fechahora.ToString("dd/MM/yyyy HH:mm"),
                    Selected = f.ID_Funcion == funcionId
                }).ToList();

            var peliculas = peliculasData
                .Select(p => new SelectListItem
                {
                    Value = p.ID_Pelicula.ToString(),
                    Text = p.PEL_Titulo,
                    Selected = p.ID_Pelicula == peliculaId
                }).ToList();

            var estadoActual = reservaActual.EstadoReserva?.ESR_Estado ?? "Pendiente";

            var estados = estadosData
                .Select(e => new SelectListItem
                {
                    Value = e.ESR_Estado,
                    Text = e.ESR_Estado,
                    Selected = e.ESR_Estado == estadoActual
                });


            var funcionSeleccionada = funcionesData.FirstOrDefault(f => f.ID_Funcion == funcionId);
            var asientosDisponibles = ObtenerAsientosDisponibles(funcionId);

            var model = new EditarReservaViewModel
            {
                ID_Reserva = reservaActual.ID_Reserva,
                ID_Pelicula = peliculaId,
                PeliculaNombre = reservaActual.Funcion?.Pelicula?.PEL_Titulo ?? "No especificada",
                Peliculas = peliculas,
                ID_Funcion = funcionId,
                FuncionFecha = funcionSeleccionada?.FUN_Fechahora.ToString("dd/MM/yyyy HH:mm") ?? "No especificada",
                Funciones = funciones,
                Precio = funcionSeleccionada?.FUN_Precio ?? reservaActual.Funcion?.FUN_Precio ?? 0,
                SalaNombre = funcionSeleccionada?.Sala?.SAL_Nombre ?? reservaActual.Funcion?.Sala?.SAL_Nombre ?? "Desconocida",
                AsientosDisponiblesLista = asientosDisponibles,
                AsientosSeleccionados = asientosActuales,
                AsientosSeleccionadosTexto = string.Join(",", asientosActuales),
                AsientosDisponibles = new MultiSelectList(asientosDisponibles, asientosActuales),
                QR = reservaActual.RES_QR,
                Estado = estadoActual,
                EstadoNombre = estadoActual,
                Estados = estados,
                PeliculaCambiada = false,
                FuncionCambiada = false
            };

            return View(model);
        }

        [HttpPost]

        public ActionResult Editar(EditarReservaViewModel model, string action)
        {
            
            model.AsientosSeleccionados = model.AsientosSeleccionados ?? new List<string>();

            if (action == "eliminarAsiento")
            {
                // Asegurarnos que la lista existe
                model.AsientosSeleccionados = model.AsientosSeleccionados ?? new List<string>();

                // Eliminar el asiento si existe
                if (!string.IsNullOrEmpty(model.AsientoAEliminar))
                {
                    model.AsientosSeleccionados.Remove(model.AsientoAEliminar);
                }

                // Redirigir con la lista actual (puede estar vacía)
                return RedirectToAction("Editar", new
                {
                    id = model.ID_Reserva,
                    idPeliculaSeleccionada = model.ID_Pelicula,
                    idFuncionSeleccionada = model.ID_Funcion,
                    asientosSeleccionados = model.AsientosSeleccionados.Any() ?
                        string.Join(",", model.AsientosSeleccionados) : "Vacio"
                });
            }

            if (action == "agregarAsiento" && !string.IsNullOrEmpty(model.NuevoAsiento))
            {
                if (!model.AsientosSeleccionados.Contains(model.NuevoAsiento))
                {
                    model.AsientosSeleccionados.Add(model.NuevoAsiento);
                }
                return RedirectToAction("Editar", new
                {
                    id = model.ID_Reserva,
                    idPeliculaSeleccionada = model.ID_Pelicula,
                    idFuncionSeleccionada = model.ID_Funcion,
                    asientosSeleccionados = string.Join(",", model.AsientosSeleccionados)
                });
            }

            if (action == "guardar")
            {
                if (ModelState.IsValid)
                {
                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var reserva = _dbContext.Reserva
                                .Include(r => r.Boleto)
                                .Include(r => r.Funcion)
                                .FirstOrDefault(r => r.ID_Reserva == model.ID_Reserva);

                            if (reserva == null) return HttpNotFound();

                            // Eliminar boletos existentes
                            _dbContext.Boleto.RemoveRange(reserva.Boleto);

                            if (model.AsientosSeleccionados != null && model.AsientosSeleccionados.Any())
                            {
                                foreach (var asientoStr in model.AsientosSeleccionados)
                                {
                                    if (!string.IsNullOrEmpty(asientoStr))
                                    {
                                        var fila = asientoStr[0].ToString();
                                        var numero = int.Parse(asientoStr.Substring(1));

                                        var asiento = _dbContext.Asiento
                                            .FirstOrDefault(a => a.ASI_Fila == fila &&
                                                               a.ASI_Numero == numero &&
                                                               a.ID_Sala == reserva.Funcion.ID_Sala);

                                        if (asiento != null)
                                        {
                                            _dbContext.Boleto.Add(new Boleto
                                            {
                                                ID_Reserva = reserva.ID_Reserva,
                                                ID_Asiento = asiento.ID_Asiento
                                            });
                                        }
                                    }
                                }
                            }


                            reserva.ID_Funcion = model.ID_Funcion;
                            reserva.RES_QR = model.QR;

                            if (!string.IsNullOrEmpty(model.Estado))
                            {
                                var estado = _dbContext.EstadoReserva
                                    .FirstOrDefault(e => e.ESR_Estado == model.Estado);
                                if (estado != null) reserva.ID_REstado = estado.ID_Estadoreserva;
                            }

                            _dbContext.SaveChanges();
                            transaction.Commit();

                            return RedirectToAction("Detalles", new { id = model.ID_Reserva });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                            
                        }
                    }
                }
            }

         
            RecargarDatosViewModel(model);
            return View(model);
        }

        private void RecargarDatosViewModel(EditarReservaViewModel model)
        {
           
            model.Peliculas = _dbContext.Pelicula
                .ToList()
                .Select(p => new SelectListItem
                {
                    Value = p.ID_Pelicula.ToString(),
                    Text = p.PEL_Titulo,
                    Selected = p.ID_Pelicula == model.ID_Pelicula
                });

          
            model.Funciones = _dbContext.Funcion
                .Where(f => f.ID_Pelicula == model.ID_Pelicula)
                .ToList()
                .Select(f => new SelectListItem
                {
                    Value = f.ID_Funcion.ToString(),
                    Text = f.FUN_Fechahora.ToString("dd/MM/yyyy HH:mm"),
                    Selected = f.ID_Funcion == model.ID_Funcion
                });

         
            model.Estados = _dbContext.EstadoReserva
                .ToList()
                .Select(e => new SelectListItem
                {
                    Value = e.ESR_Estado,
                    Text = e.ESR_Estado,
                    Selected = e.ESR_Estado == model.Estado
                });

          
            model.AsientosDisponiblesLista = ObtenerAsientosDisponibles(model.ID_Funcion);
        }
        private List<string> ObtenerAsientosDisponibles(int idFuncion)
        {
            if (idFuncion <= 0) return new List<string>();

           
            var asientosOcupadosIds = _dbContext.Boleto
                .Where(b => b.Reserva.ID_Funcion == idFuncion && b.ID_Asiento != null)
                .Select(b => b.ID_Asiento.Value)
                .ToList();

         
            var funcionConSala = _dbContext.Funcion
                .Include("Sala") 
                .FirstOrDefault(f => f.ID_Funcion == idFuncion);

            if (funcionConSala?.Sala == null) return new List<string>();

        
            var todosAsientos = _dbContext.Asiento
                .Where(a => a.ID_Sala == funcionConSala.Sala.ID_Sala)
                .ToList();

       
            var asientosDisponibles = todosAsientos
                .Where(a => !asientosOcupadosIds.Contains(a.ID_Asiento))
                .OrderBy(a => a.ASI_Fila)
                .ThenBy(a => a.ASI_Numero)
                .Select(a => $"{a.ASI_Fila}{a.ASI_Numero}")
                .ToList();

            return asientosDisponibles;
        }


      
       
    }
}