using Cinemax.Models;
using Cinemax.Servicios;
using Cinemax.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Cinemax.Controllers
{
    public class ClienteController : Controller
    {
        private CinemaxEntities db = new CinemaxEntities();

        public ActionResult Historial(string Busqueda, int pagina = 1)
        {
            int idCliente = (int)Session["usuarioId"];
            int reservasPorPagina = 10;

            var query = from r in db.Reserva
                        join f in db.Funcion on r.ID_Funcion equals f.ID_Funcion
                        join p in db.Pelicula on f.ID_Pelicula equals p.ID_Pelicula
                        join s in db.Sala on f.ID_Sala equals s.ID_Sala
                        join er in db.EstadoReserva on r.ID_REstado equals er.ID_Estadoreserva
                        join b in db.Boleto on r.ID_Reserva equals b.ID_Reserva into boletos
                        from b in boletos.DefaultIfEmpty()
                        join a in db.Asiento on (b == null ? null : (int?)b.ID_Asiento) equals a.ID_Asiento into asientos
                        from a in asientos.DefaultIfEmpty()
                        where r.ID_Usuario == idCliente
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

            var datos = query
              .GroupBy(x => x.r.ID_Reserva)
              .AsEnumerable() // 👈 Esto fuerza a que el resto se ejecute en memoria (LINQ to Objects)
              .Select(g => new HistorialReservaViewModel
              {
                  Id = g.Key,
                  Fecha = g.Select(x => x.r.RES_Reserva).FirstOrDefault().ToShortDateString(), // ✅ Ahora sí es válido
                  Pelicula = g.Select(x => x.p.PEL_Titulo).FirstOrDefault(),
                  Precio = g.Select(x => x.f.FUN_Precio).FirstOrDefault(),
                  Sala = g.Select(x => x.s.SAL_Nombre).FirstOrDefault(),
                  Asientos = string.Join(", ", g.Where(x => x.a != null)
                                                .Select(x => x.a.ASI_Fila + x.a.ASI_Numero)
                                                .Distinct()),
                  CodigoQR = g.Select(x => x.r.RES_QR).FirstOrDefault(),
                  Estado = g.Select(x => x.er.ESR_Estado).FirstOrDefault()
              }).ToList();

            int total = datos.Count();
            int totalPaginas = (int)Math.Ceiling((double)total / reservasPorPagina);
            pagina = Math.Max(1, Math.Min(pagina, totalPaginas));

            var paginadas = datos.Skip((pagina - 1) * reservasPorPagina).Take(reservasPorPagina).ToList();

            ViewBag.BusquedaActual = Busqueda;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(paginadas);
        }

    }
}
