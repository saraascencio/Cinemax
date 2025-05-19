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
                .Include("Pelicula")
                .Include("Sala")
                .FirstOrDefault(f => f.ID_Funcion == id);

            if (funcion == null)
                return HttpNotFound();

            return View(funcion);
        }
    }
}