using Cinemax.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Cinemax.Controllers
{
    public class PagoController : Controller
    {
        private CinemaxEntities db = new CinemaxEntities();


        /*
        // hacerlo desde el controlador que se va a llamar
return RedirectToAction("Pago", "Pago", new { id_Reserva = reservaId });
        */
        public ActionResult Pago(int id_Reserva=41)
        {
            TempData["id_Reserva"] = id_Reserva;
            return View();
        }

        [HttpPost]
        public ActionResult ProcesarPago(FormCollection form)
        {
            try
            {
                int id_Reserva = (int)TempData["id_Reserva"];
                string email = form["email"];
                int metodoPago = Convert.ToInt32(form["metodoPago"]);
                if (!email.Contains(".com"))
                {
                    TempData["ErrorPago"] = "Su pago ha fallado. Tu pago a través de PayPal no se pudo completar. Esto puede deberse a fondos insuficientes, un problema con la tarjeta asociada, restricciones en tu cuenta o un error temporal en la plataforma.";
                    return RedirectToAction("Pago", new { id_Reserva = id_Reserva });
                }
                int idMetodo = metodoPago == 2 ? 2 : 3;

                var nuevoPago = new Pago
                {
                    ID_Reserva = id_Reserva,
                    ID_Metodo = idMetodo,
                    ID_Estadopago = 1,
                    PAG_Fecha = DateTime.Now
                };

                db.Pago.Add(nuevoPago);
                db.SaveChanges();

                return RedirectToAction("PagoAprovado");
            }
            catch (Exception ex)
            {
                TempData["ErrorPago"] = "Ocurrió un error al procesar el pago: " + ex.Message;
                return RedirectToAction("Pago", new { id_Reserva = TempData["id_Reserva"] });
            }
        }

        public ActionResult PagoAprovado()
        {
            return View();
        }
    }
}