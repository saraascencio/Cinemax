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
    public class PagoController : Controller
    {
        private CinemaxEntities db = new CinemaxEntities();

        public ActionResult Pago() { 
            return View();
        }

        public ActionResult PagoAprovado()
        {
            return View();
        }
    }
}