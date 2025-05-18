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
        public ActionResult Index(DateTime? fecha, string genero)
        {
            DateTime fechaBusqueda = fecha?.Date ?? DateTime.Today;

            var funciones = db.Funcion
         .Include(f => f.Pelicula.Genero)
         .Where(f => DbFunctions.TruncateTime(f.FUN_Fechahora) == fechaBusqueda)
         .ToList()
         .Where(f => string.IsNullOrEmpty(genero) || f.Pelicula.Genero.GEN_Nombre == genero)
         .GroupBy(f => f.Pelicula)
         .Select(g => g.Key)
         .ToList();

            ViewBag.Generos = db.Genero.Select(g => g.GEN_Nombre).ToList();
            ViewBag.FechaSeleccionada = fechaBusqueda;
            ViewBag.GeneroSeleccionado = genero;
            return View(funciones);
        }
    }
}