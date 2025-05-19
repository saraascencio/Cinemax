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
    public class CarteleraController : Controller
    {
        private CinemaxEntities db = new CinemaxEntities();

        // GET: Cartelera
        public ActionResult Index(DateTime? fecha, string genero, int pagina = 1)
        {
            DateTime fechaBusqueda = fecha?.Date ?? DateTime.Today;
            int funcionesPorPagina = 6;

            var funciones = db.Funcion
                .Include(f => f.Pelicula.Genero)
                .Include(f => f.Sala)
                .Where(f => DbFunctions.TruncateTime(f.FUN_Fechahora) == fechaBusqueda)
                .ToList()
                .Where(f => string.IsNullOrEmpty(genero) || f.Pelicula.Genero.GEN_Nombre == genero)
                .ToList();

            int totalFunciones = funciones.Count();
            int totalPaginas = (int)Math.Ceiling((double)totalFunciones / funcionesPorPagina);

            var funcionesPaginadas = funciones
                .Skip((pagina - 1) * funcionesPorPagina)
                .Take(funcionesPorPagina)
                .ToList();

            var viewModel = new CarteleraViewModel
            {
                Funciones = funcionesPaginadas,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas
            };

            ViewBag.Generos = db.Genero.Select(g => g.GEN_Nombre).ToList();
            ViewBag.FechaSeleccionada = fechaBusqueda;
            ViewBag.GeneroSeleccionado = genero;

            return View(viewModel);
        }

    }
}