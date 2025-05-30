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
            // 1. Obtener función con su sala
            var funcion = db.Funcion
                .Include(f => f.Sala)
                .Include(f => f.Pelicula)
                .FirstOrDefault(f => f.ID_Funcion == id);

            if (funcion == null) return HttpNotFound();

            int salaId = funcion.ID_Sala.Value;

            // 2. Obtener todos los asientos de la sala
            var asientosSala = db.Asiento
                .Where(a => a.ID_Sala == salaId)
                .ToList();

            // 3. Obtener los asientos ocupados para esta función
            var asientosOcupados = db.FuncionAsiento
                .Where(fa => fa.ID_Funcion == id && fa.ID_EstadoAsiento == 2) // 2 = ocupado (ajusta según tu tabla)
                .Select(fa => fa.ID_Asiento)
                .ToList();

            // 4. Crear ViewModel con estado
            var asientosConEstado = asientosSala.Select(a => new FuncionAsiento
            {
                ID_Asiento = a.ID_Asiento,
                Asiento = a,
                ID_Funcion = id,
                ID_EstadoAsiento = asientosOcupados.Contains(a.ID_Asiento) ? 2 : 1 // 1 = disponible
            }).ToList();

            var vm = new FuncionDetalleViewModel
            {
                Funcion = funcion,
                Asientos = asientosConEstado
            };

            return View(vm);
        }
    }
}