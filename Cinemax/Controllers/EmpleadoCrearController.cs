using Cinemax.Models;
using Cinemax.Servicios;
using Cinemax.ViewModels;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.Controllers
{
    public class EmpleadoCrearController : Controller
    {
        private CinemaxEntities _dbContext;

        public EmpleadoCrearController()
        {
            _dbContext = new CinemaxEntities();
        }

        [Autenticacion]
        public ActionResult Crear()
        {
            var model = new CrearReservaEViewModel
            {
                Peliculas = _dbContext.Pelicula.Select(p => new SelectListItem
                {
                    Value = p.ID_Pelicula.ToString(),
                    Text = p.PEL_Titulo
                }),
                MetodosPago = _dbContext.MetodoPago.Select(m => new SelectListItem
                {
                    Value = m.ID_Metodo.ToString(),
                    Text = m.MET_Nombre
                })
            };

            return View(model);
        }

        [Autenticacion]
        public JsonResult BuscarCliente(string email)
        {
            try
            {
                
                System.Diagnostics.Debug.WriteLine($"Buscando cliente con email: {email}");

                
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Json(new
                    {
                        success = false,
                        message = "El email no puede estar vacío"
                    });
                }

        
                var cliente = _dbContext.Usuario
                    .Include(u => u.Rol)
                    .FirstOrDefault(u => u.USU_Email.Equals(email.Trim()));

           
                System.Diagnostics.Debug.WriteLine($"Resultado de búsqueda: {(cliente == null ? "No encontrado" : "Encontrado")}");

               
                if (cliente != null && cliente.ID_Rol == 3)
                {
                    return Json(new
                    {
                        success = true,
                        usuarioId = cliente.ID_Usuario,
                        nombreCliente = cliente.USU_Nombre,
                        emailCliente2 = cliente.USU_Email
                    });
                }

                
                return Json(new
                {
                    success = false,
                    message = cliente == null ? "Usuario no encontrado" : "El usuario no es un cliente válido"
                });
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"Error en BuscarCliente: {ex.Message}");

                
                return Json(new
                {
                    success = false,
                    message = "Error al buscar el cliente: " + ex.Message
                });
            }
        }

        [Autenticacion]

        [HttpPost]
        public JsonResult ObtenerFechasDisponibles(int peliculaId)
        {
            var fechas = _dbContext.Funcion
                .Where(f => f.ID_Pelicula == peliculaId && f.FUN_Fechahora > DateTime.Now)
                .ToList() 
                .Select(f => f.FUN_Fechahora.Date) 
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            return Json(fechas.Select(f => f.ToString("yyyy-MM-dd"))); 
        }

        [Autenticacion]

        [HttpPost]
        public JsonResult ObtenerHorasDisponibles(int peliculaId, string fechaSeleccionada)
        {
            if (!DateTime.TryParse(fechaSeleccionada, out DateTime fecha))
            {
                return Json(new { success = false, message = "Fecha inválida" });
            }

  
            var funciones = _dbContext.Funcion
                .Include(f => f.Sala)
                .Where(f => f.ID_Pelicula == peliculaId &&
                           DbFunctions.TruncateTime(f.FUN_Fechahora) == fecha.Date)
                .OrderBy(f => f.FUN_Fechahora)
                .ToList(); 

            var horas = funciones.Select(f => new {
                FuncionId = f.ID_Funcion,
                Hora = f.FUN_Fechahora.ToString("HH:mm"),
                Precio = f.FUN_Precio,
                SalaNombre = f.Sala?.SAL_Nombre
            }).ToList();

            return Json(new { success = true, horas });
        }

        [Autenticacion]

        [HttpPost]
        public JsonResult ObtenerDetallesFuncion(int funcionId)
        {
            var funcion = _dbContext.Funcion
                .Include(f => f.Sala) 
                .FirstOrDefault(f => f.ID_Funcion == funcionId);

            if (funcion == null)
            {
                return Json(new { success = false, message = "Función no encontrada" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    precio = funcion.FUN_Precio,
                    salaNombre = funcion.Sala?.SAL_Nombre 
                }
            });
        }

        [Autenticacion]
        [HttpPost]

        public JsonResult ObtenerAsientosDisponibles(int funcionId)
        {
          
            var salaId = _dbContext.Funcion.Find(funcionId).ID_Sala;
            var todosAsientos = _dbContext.Asiento
                .Where(a => a.ID_Sala == salaId)
                .OrderBy(a => a.ASI_Fila)
                .ThenBy(a => a.ASI_Numero)
                .ToList();

           
            var asientosOcupadosIds = _dbContext.FuncionAsiento
                .Where(fa => fa.ID_Funcion == funcionId && fa.ID_EstadoAsiento == 2)
                .Select(fa => fa.ID_Asiento)
                .ToList();

          
            var asientosConEstado = todosAsientos.Select(a => new
            {
                Id = a.ID_Asiento,
                Nombre = $"{a.ASI_Fila}{a.ASI_Numero}",
                Fila = a.ASI_Fila,
                Numero = a.ASI_Numero,
                Estado = asientosOcupadosIds.Contains(a.ID_Asiento) ? "ocupado" : "disponible"
            }).ToList();

            return Json(asientosConEstado);
        }

        [Autenticacion]

        [HttpPost]
        public ActionResult Crear(CrearReservaEViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var funcion = _dbContext.Funcion
                .Include(f => f.Sala)
                 .FirstOrDefault(f => f.ID_Funcion == model.FuncionId);

                        if (funcion == null)
                        {
                            throw new Exception("La función seleccionada no existe");
                        }

                        string qrCode = $"QR{DateTime.Now:MMdd}";

                        var reserva = new Reserva
                        {
                            ID_Funcion = model.FuncionId,
                            ID_Usuario = model.UsuarioId,
                            ID_REstado = 2,
                            RES_Reserva = DateTime.Now,
                            RES_QR = qrCode
                        };

                        //De esta parte podemos destacar que si el cliente no es 
                        // un usuario ya registrado, acá se guardaría
                        bool esClienteExistente = !string.IsNullOrEmpty(model.UsuarioId?.ToString());

                        if (!esClienteExistente)
                        {
                            reserva.Nombre_Cliente = model.NombreCliente;
                            reserva.Telefono_Cliente = model.TelefonoCliente;
                            reserva.Email_Cliente = model.EmailCliente2;
                        }

                        _dbContext.Reserva.Add(reserva);
                        _dbContext.SaveChanges();

                        reserva.RES_QR = $"QR{reserva.ID_Reserva}{DateTime.Now:MMdd}";
                        _dbContext.SaveChanges();


                        if (!string.IsNullOrEmpty(model.AsientosSeleccionados))
                        {
                            var asientos = model.AsientosSeleccionados.Split(',')
                                .Select(a => a.Trim())
                                .Where(a => !string.IsNullOrEmpty(a))
                                .ToList();

                            foreach (var asientoStr in asientos)
                            {
                                var fila = asientoStr[0].ToString();
                                var numero = int.Parse(asientoStr.Substring(1));

                                var asiento = _dbContext.Asiento.FirstOrDefault(a =>
                                    a.ASI_Fila == fila &&
                                    a.ASI_Numero == numero &&
                                    a.ID_Sala == funcion.ID_Sala);

                                if (asiento != null)
                                {
                                    _dbContext.Boleto.Add(new Boleto
                                    {
                                        ID_Reserva = reserva.ID_Reserva,
                                        ID_Asiento = asiento.ID_Asiento
                                    });

                                    var funcionAsiento = _dbContext.FuncionAsiento
                                        .FirstOrDefault(fa => fa.ID_Funcion == model.FuncionId &&
                                                            fa.ID_Asiento == asiento.ID_Asiento);

                                    if (funcionAsiento != null)
                                    {
                                        funcionAsiento.ID_EstadoAsiento = 2;
                                    }
                                    else
                                    {
                                        _dbContext.FuncionAsiento.Add(new FuncionAsiento
                                        {
                                            ID_Funcion = model.FuncionId.Value,
                                            ID_Asiento = asiento.ID_Asiento,
                                            ID_EstadoAsiento = 2
                                        });
                                    }
                                }
                            }
                        }


                        _dbContext.Pago.Add(new Pago
                        {
                            ID_Reserva = reserva.ID_Reserva,
                            ID_Metodo = model.MetodoPagoId,
                            ID_Estadopago = 1,
                            PAG_Fecha = DateTime.Now
                        });

                        _dbContext.SaveChanges();
                        transaction.Commit();

                        return RedirectToAction("Confirmacion", new { id = reserva.ID_Reserva });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", $"Error al procesar la reserva: {ex.Message}");

                    }
                }
            }

            RecargarListas(model);
            return View(model);
        }
        private void RecargarListas(CrearReservaEViewModel model)
        {
            model.Peliculas = _dbContext.Pelicula
                .Select(p => new SelectListItem
                {
                    Value = p.ID_Pelicula.ToString(),
                    Text = p.PEL_Titulo
                }).ToList();

            model.MetodosPago = _dbContext.MetodoPago
                .Select(m => new SelectListItem
                {
                    Value = m.ID_Metodo.ToString(),
                    Text = m.MET_Nombre
                }).ToList();
        }


        [Autenticacion]
        public ActionResult Confirmacion(int id)
        {
            var reserva = _dbContext.Reserva
                .Include(r => r.Funcion)
                .Include(r => r.Funcion.Pelicula)
                .Include(r => r.Funcion.Sala)
                .Include(r => r.Boleto.Select(b => b.Asiento))
                .Include(r => r.Pago.Select(p => p.MetodoPago)) 
                .Include(r => r.Pago.Select(p => p.EstadoPago)) 
                .FirstOrDefault(r => r.ID_Reserva == id);

            if (reserva == null)
            {
                return HttpNotFound();
            }

            ViewBag.QRCodeImage = GenerarQRCode(reserva.RES_QR);

            return View(reserva);
        }

    
        private string GenerarQRCode(string textoQR)
        {
            if (string.IsNullOrEmpty(textoQR))
                return null;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQR, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}