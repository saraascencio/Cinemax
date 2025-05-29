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
    public class EmpleadoHistorialController : Controller
    {

        private CinemaxEntities _dbContext;

        public EmpleadoHistorialController()
        {
            _dbContext = new CinemaxEntities();
        }

        [Autenticacion]
        public ActionResult HistorialReservas(string Busqueda, int pagina = 1)
        {
            int reservasPorPagina = 25;

          
            var query = from r in _dbContext.Reserva
                        join f in _dbContext.Funcion on r.ID_Funcion equals f.ID_Funcion
                        join p in _dbContext.Pelicula on f.ID_Pelicula equals p.ID_Pelicula
                        join s in _dbContext.Sala on f.ID_Sala equals s.ID_Sala
                        join er in _dbContext.EstadoReserva on r.ID_REstado equals er.ID_Estadoreserva
                        join b in _dbContext.Boleto on r.ID_Reserva equals b.ID_Reserva into boletos
                        from b in boletos.DefaultIfEmpty()
                        join a in _dbContext.Asiento on (b == null ? null : (int?)b.ID_Asiento) equals a.ID_Asiento into asientos
                        from a in asientos.DefaultIfEmpty()
                        select new { r, f, p, s, er, a };

            if (!string.IsNullOrEmpty(Busqueda))
            {
                if (int.TryParse(Busqueda, out int idBuscado))
                {
                    query = query.Where(x => x.r.ID_Reserva == idBuscado);
                }
                else
                {
                    query = query.Where(x => x.p.PEL_Titulo.Contains(Busqueda) || x.r.RES_QR.Contains(Busqueda));
                }
            }

            var datosReservas = query
                .GroupBy(x => x.r.ID_Reserva)
                .Select(g => new
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
                })
                .AsEnumerable();

            var todasLasReservas = datosReservas.Select(x => new ReservaViewModel
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

         
            int totalReservas = todasLasReservas.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalReservas / reservasPorPagina);

           
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

        
            var reservasPaginadas = todasLasReservas
                .Skip((pagina - 1) * reservasPorPagina)
                .Take(reservasPorPagina)
                .ToList();

            
            ViewBag.BusquedaActual = Busqueda;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalReservas = totalReservas;
            ViewBag.ReservasPorPagina = reservasPorPagina;

            return View(reservasPaginadas);
        }




        [Autenticacion]
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

            
            if (asientosSeleccionados == "Vacio")
            {
                asientosActuales = new List<string>();
            }
     
            else if (!string.IsNullOrEmpty(asientosSeleccionados))
            {
                asientosActuales = asientosSeleccionados.Split(',').ToList();
            }
       
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


        [Autenticacion]
        [HttpPost]

        public ActionResult Editar(EditarReservaViewModel model, string action)
        {

            model.AsientosSeleccionados = model.AsientosSeleccionados ?? new List<string>();

            if (action == "eliminarAsiento")
            {
               
                model.AsientosSeleccionados = model.AsientosSeleccionados ?? new List<string>();

               
                if (!string.IsNullOrEmpty(model.AsientoAEliminar))
                {
                    model.AsientosSeleccionados.Remove(model.AsientoAEliminar);
                }

                
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

                            return RedirectToAction("Detalles", "EmpleadoPDF", new { id = model.ID_Reserva });
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

        [Autenticacion]
        [HttpPost]
        public JsonResult ObtenerAsientosDisponiblesParaEdicion(int funcionId, int reservaId)
        {
            var salaId = _dbContext.Funcion.Find(funcionId).ID_Sala;

            var todosAsientos = _dbContext.Asiento
                .Where(a => a.ID_Sala == salaId)
                .OrderBy(a => a.ASI_Fila)
                .ThenBy(a => a.ASI_Numero)
                .ToList();

            // Obtener asientos ocupados (excluyendo los de esta reserva)
            var asientosOcupadosIds = _dbContext.Boleto
                .Where(b => b.Reserva.ID_Funcion == funcionId &&
                           b.Reserva.ID_Reserva != reservaId &&
                           b.ID_Asiento != null)
                .Select(b => b.ID_Asiento.Value)
                .Distinct()
                .ToList();

            // Obtener asientos de esta reserva
            var asientosReservadosIds = _dbContext.Boleto
                .Where(b => b.Reserva.ID_Reserva == reservaId && b.ID_Asiento != null)
                .Select(b => b.ID_Asiento.Value)
                .Distinct()
                .ToList();

            var asientosConEstado = todosAsientos.Select(a => new
            {
                Id = a.ID_Asiento,
                Nombre = $"{a.ASI_Fila}{a.ASI_Numero}",
                Fila = a.ASI_Fila,
                Numero = a.ASI_Numero,
                Estado = asientosReservadosIds.Contains(a.ID_Asiento) ? "reserved" :
                        asientosOcupadosIds.Contains(a.ID_Asiento) ? "occupied" : "available"
            }).ToList();

            return Json(asientosConEstado);
        }

        [Autenticacion]
        [HttpPost]
        public ActionResult CancelarReserva(int id)
        {
            var reserva = _dbContext.Reserva.FirstOrDefault(r => r.ID_Reserva == id);
            if (reserva == null)
            {
                return HttpNotFound();
            }

            var estadoCancelado = _dbContext.EstadoReserva.FirstOrDefault(e => e.ESR_Estado == "Cancelada");
            if (estadoCancelado == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "No se encontró el estado 'Cancelada'.");
            }

            reserva.ID_REstado = estadoCancelado.ID_Estadoreserva;
            _dbContext.SaveChanges();

            TempData["Mensaje"] = "La reserva fue cancelada correctamente.";
            return RedirectToAction("Detalles", "EmpleadoPDF", new { id = reserva.ID_Reserva });

        }



    }
}
    
