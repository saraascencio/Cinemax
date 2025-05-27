using Cinemax.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Windows.Documents;

namespace Cinemax.Controllers
{
    public class PagoController : Controller
    {
        private CinemaxEntities db = new CinemaxEntities();

        public ActionResult Pago(int id_funcion = 30, int id_Usuario = 1, List<int> asientos = null)
        {
            if (asientos == null)
            {
                asientos = new List<int> { 1, 2, 3, 4 };
            }

            TempData["id_Funcion"] = id_funcion;
            TempData["id_Usuario"] = id_Usuario;
            TempData["Asientos"] = asientos;
            return View();
        }

        [HttpPost]
        public ActionResult ProcesarPago(FormCollection form)
        {
            try
            {
                int id_funcion = (int)TempData["id_Funcion"];
                int id_usuario = (int)TempData["id_Usuario"];
                List<int> asientos = TempData["Asientos"] as List<int>;

                string nombre = form["nombre"];
                string apellido = form["apellido"];
                string email = form["email"];
                string telefono = form["telefono"];
                int metodoPago = Convert.ToInt32(form["metodoPago"]);

                // Validaciones básicas
                if (!email.Contains(".com") || telefono.Length < 8 || telefono.Length > 12)
                {
                    TempData["ErrorPago"] = "Ingrese un correo válido y número de teléfono correcto.";
                    return RedirectToAction("Pago", new { id_funcion, id_usuario, asientos });
                }

                // Generar el RES_QR automáticamente
                string nuevoQR = "QR10"; // valor por defecto si no hay registros

                var ultimoQR = db.Reserva
                                 .Where(r => r.RES_QR.StartsWith("QR"))
                                 .OrderByDescending(r => r.ID_Reserva)
                                 .Select(r => r.RES_QR)
                                 .FirstOrDefault();

                if (!string.IsNullOrEmpty(ultimoQR) && ultimoQR.Length > 2)
                {
                    string numeroStr = ultimoQR.Substring(2);
                    if (int.TryParse(numeroStr, out int numero))
                    {
                        int nuevoNumero = numero + 10;
                        nuevoQR = "QR" + nuevoNumero.ToString();
                    }
                }

                // Crear Reserva
                var nuevaReserva = new Reserva
                {
                    ID_Funcion = id_funcion,
                    ID_Usuario = id_usuario,
                    ID_REstado = 1, // estado inicial
                    RES_Reserva = DateTime.Now,
                    RES_QR = nuevoQR,
                    Nombre_Cliente = nombre + " " + apellido,
                    Telefono_Cliente = telefono,
                    Email_Cliente = email
                };

                db.Reserva.Add(nuevaReserva);
                db.SaveChanges();

                int idReserva = nuevaReserva.ID_Reserva;

                // Crear Pago
                var nuevoPago = new Pago
                {
                    ID_Reserva = idReserva,
                    ID_Metodo = metodoPago,
                    ID_Estadopago = 1,
                    PAG_Fecha = DateTime.Now
                };

                db.Pago.Add(nuevoPago);
                db.SaveChanges();

                // Crear Boletos
                foreach (var idAsiento in asientos)
                {
                    var boleto = new Boleto
                    {
                        ID_Reserva = idReserva,
                        ID_Asiento = idAsiento
                    };
                    db.Boleto.Add(boleto);
                }
                db.SaveChanges();

                return RedirectToAction("PagoAprovado");
            }
            catch (Exception ex)
            {
                TempData["ErrorPago"] = "Ocurrió un error al procesar la reserva y pago: " + ex.Message;
                return RedirectToAction("Pago", new { id_funcion = TempData["id_Funcion"], id_usuario = TempData["id_Usuario"], asientos = TempData["Asientos"] });
            }
        }


        public ActionResult PagoAprovado()
        {
            return View();
        }
    }
}