using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.Controllers
{
    public class HomeController : Controller
    {
       
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult DashboardCliente()
        {
            return View();
        }



        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
           
            if (Username == "empleado@gmail.com" && Password == "12345")
            {
                
                return RedirectToAction("Dashboard");
            }
            else if (Username == "cliente@gmail.com" && Password == "12345")
            {
                
                return RedirectToAction("DashboardCliente");
            }
            else
            {
               
                ViewBag.ErrorMessage = "Credenciales incorrectas. Inténtalo de nuevo.";
                return View();
            }
        }




        public ActionResult Registro()
        {
            return View(); 
        }
    }
}