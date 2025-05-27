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
    public class FuncionesController : Controller
    {
        private CinemaxEntities db = new CinemaxEntities();

        public ActionResult Detalles(int id)
        {
            var funcion = db.Funcion
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                .FirstOrDefault(f => f.ID_Funcion == id);

            var asientos = db.FuncionAsiento
                .Include(fa => fa.Asiento)          
                .Where(fa => fa.ID_Funcion == id)
                .ToList();

            var vm = new FuncionDetalleViewModel
            {
                Funcion = funcion,
                Asientos = asientos
            };
            return View(vm);
        }
    }
}