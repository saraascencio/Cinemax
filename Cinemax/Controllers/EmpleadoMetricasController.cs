using Cinemax.Models;
using Cinemax.Servicios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.Controllers
{
    public class EmpleadoMetricasController : Controller
    {
        // GET: EmpleadoMetricas

        private CinemaxEntities _dbContext;

        public EmpleadoMetricasController()
        {
            _dbContext = new CinemaxEntities();
        }

        [Autenticacion]
        public ActionResult Dashboard(DateTime? fechaInicio = null, DateTime? fechaFin = null, bool soloDatos = false)
        {
            if (Session["TipoUsuario"] as string != "Empleado")
            {
                return RedirectToAction("Login", "Home");
            }

         
                ViewBag.Nombre = Session["Nombre"];
                ViewBag.TipoUsuario = Session["TipoUsuario"];
            
            // 1. Ganancias totales con filtro opcional
            var reservasQuery = from r in _dbContext.Reserva
                                join f in _dbContext.Funcion on r.ID_Funcion equals f.ID_Funcion
                                join b in _dbContext.Boleto on r.ID_Reserva equals b.ID_Reserva into boletos
                                select new
                                {
                                    Reserva = r,
                                    Funcion = f,
                                    Boletos = boletos.ToList()
                                };

            if (fechaInicio != null && fechaFin != null)
            {
                reservasQuery = reservasQuery.Where(r => r.Reserva.RES_Reserva >= fechaInicio && r.Reserva.RES_Reserva <= fechaFin);
            }

            var reservas = reservasQuery.ToList();

            ViewBag.GananciasTotales = reservas.Sum(r => r.Funcion.FUN_Precio * r.Boletos.Count);

            
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                var gananciasMesQuery = from r in _dbContext.Reserva
                                        join f in _dbContext.Funcion on r.ID_Funcion equals f.ID_Funcion
                                        join b in _dbContext.Boleto on r.ID_Reserva equals b.ID_Reserva into boletos
                                        where r.RES_Reserva >= inicioMes && r.RES_Reserva <= finMes
                                        select new
                                        {
                                            Funcion = f,
                                            Boletos = boletos.ToList()
                                        };

                ViewBag.GananciasMesActual = gananciasMesQuery.ToList().Sum(r => r.Funcion.FUN_Precio * r.Boletos.Count);
           
            // 3. Porcentajes
            ViewBag.PorcentajeClientesRegistrados = CalcularPorcentaje(
                reservas.Count(r => r.Reserva.ID_Usuario != null),
                reservas.Count
            );

            ViewBag.PorcentajeConfirmadas = CalcularPorcentaje(
                reservas.Count(r => r.Reserva.ID_REstado == 2),
                reservas.Count
            );

            ViewBag.PorcentajeCanceladas = CalcularPorcentaje(
                reservas.Count(r => r.Reserva.ID_REstado == 3),
                reservas.Count
            );

           
            ViewBag.LabelsBarras = new[] { "Confirmadas", "Pendientes", "Canceladas" };
            ViewBag.DatosBarras = new[] {
            reservas.Count(r => r.Reserva.ID_REstado == 2),
            reservas.Count(r => r.Reserva.ID_REstado == 1),
            reservas.Count(r => r.Reserva.ID_REstado == 3)
            };

        
            var pagosQuery = from p in _dbContext.Pago
                             join m in _dbContext.MetodoPago on p.ID_Metodo equals m.ID_Metodo into metodo
                             from m in metodo.DefaultIfEmpty()
                             select new
                             {
                                 Pago = p,
                                 MetodoNombre = m != null ? m.MET_Nombre : "Desconocido"
                             };

            if (fechaInicio != null && fechaFin != null)
            {
                pagosQuery = pagosQuery.Where(p => p.Pago.PAG_Fecha >= fechaInicio && p.Pago.PAG_Fecha <= fechaFin);
            }

            var pagos = pagosQuery.ToList();

            ViewBag.LabelsPie = pagos
                .GroupBy(p => p.MetodoNombre)
                .Select(g => g.Key)
                .ToArray();

            ViewBag.DatosPie = pagos
                .GroupBy(p => p.MetodoNombre)
                .Select(g => g.Count())
                .ToArray();

            ViewBag.FechaInicioParam = fechaInicio;
            ViewBag.FechaFinParam = fechaFin;
            if (!soloDatos)
            {
                return View();
            }
            return null;
        }

        private double CalcularPorcentaje(int cantidad, int total)
        {
            return total > 0 ? Math.Round((double)cantidad / total * 100, 2) : 0;
        }


        [Autenticacion]
        public ActionResult ImprimirDashboard(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

          
            Dashboard(fechaInicio, fechaFin, false);

            return new Rotativa.ViewAsPdf("DashboardImpresion")
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type --header-center \"Reporte de Dashboard\" --footer-center \"Página [page] de [toPage]\" --footer-font-size 8"
            };
        }

        [Autenticacion]
        public ActionResult DashboardImpresion(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
       
            Dashboard(fechaInicio, fechaFin, false);
            return View();
        }

    }
}