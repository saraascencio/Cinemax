using Cinemax.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Windows.Documents;

namespace Cinemax.Controllers
{
    public class PagoController : Controller
    {
        private CinemaxEntities db = new CinemaxEntities();

        public ActionResult Pago(int id_funcion, int id_Usuario, List<int> asientos)
        {
            var funcion = db.Funcion.FirstOrDefault(f => f.ID_Funcion == id_funcion);

            TempData["id_Funcion"] = id_funcion;
            TempData["id_Usuario"] = id_Usuario;
            TempData["Asientos"] = asientos;
            if(asientos == null || asientos.Count == 0)
            {
                asientos = Session["Asientos2"] as List<int>;
                TempData["Asientos"] = asientos;
            }
            ViewBag.PrecioXasiento = funcion.FUN_Precio;
            ViewBag.Total = funcion.FUN_Precio * asientos.Count;
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

                if (!email.Contains(".com") || telefono.Length != 8)
                {
                    TempData["ErrorPago"] = "Ingrese un correo válido y número de teléfono correcto.";
                    Session["Asientos2"] = asientos;
                    return RedirectToAction("Pago", new { id_funcion, id_usuario });
                }

                string nuevoQR = "QR10";
                var ultimoQR = db.Reserva.Where(r => r.RES_QR.StartsWith("QR"))
                                         .OrderByDescending(r => r.ID_Reserva)
                                         .Select(r => r.RES_QR)
                                         .FirstOrDefault();

                if (!string.IsNullOrEmpty(ultimoQR) && ultimoQR.Length > 2)
                {
                    if (int.TryParse(ultimoQR.Substring(2), out int numero))
                    {
                        nuevoQR = "QR" + (numero + 10).ToString();
                    }
                }

                var nuevaReserva = new Reserva
                {
                    ID_Funcion = id_funcion,
                    ID_Usuario = id_usuario,
                    ID_REstado = 1,
                    RES_Reserva = DateTime.Now,
                    RES_QR = nuevoQR,
                    Nombre_Cliente = nombre + " " + apellido,
                    Telefono_Cliente = telefono,
                    Email_Cliente = email
                };

                db.Reserva.Add(nuevaReserva);
                db.SaveChanges();
                int idReserva = nuevaReserva.ID_Reserva;

                var nuevoPago = new Pago
                {
                    ID_Reserva = idReserva,
                    ID_Metodo = metodoPago,
                    ID_Estadopago = 1,
                    PAG_Fecha = DateTime.Now
                };
                db.Pago.Add(nuevoPago);
                db.SaveChanges();

                foreach (var idAsiento in asientos)
                {
                    db.Boleto.Add(new Boleto
                    {
                        ID_Reserva = idReserva,
                        ID_Asiento = idAsiento
                    });
                    db.FuncionAsiento.Add(new FuncionAsiento
                    {
                        ID_Funcion = id_funcion,
                        ID_Asiento = idAsiento,
                        ID_EstadoAsiento = 2 
                    });

                }
                db.SaveChanges();

                // Obtener datos para QR y correo
                var funcion = db.Funcion.FirstOrDefault(f => f.ID_Funcion == id_funcion);
                var pelicula = db.Pelicula.FirstOrDefault(p => p.ID_Pelicula == funcion.ID_Pelicula);
                var asientosInfo = db.Asiento.Where(a => asientos.Contains(a.ID_Asiento))
                                             .Select(a => a.ASI_Fila + a.ASI_Numero.ToString()).ToList();

                string datosQR = $"Reserva: {nuevoQR}\n" +
                 $"Cliente: {nombre} {apellido}\n" +
                 $"Pelicula: {pelicula.PEL_Titulo}\n" +
                 $"Asientos: {string.Join(", ", asientosInfo)}\n" +
                 $"Precio por entrada: ${funcion.FUN_Precio}\n" +
                 $"Total: ${funcion.FUN_Precio * asientos.Count}";




                // Generar imagen QR
                var qrGenerator = new QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(datosQR, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrData);

                using (Bitmap qrImage = qrCode.GetGraphic(20))
                {
                    string qrPath = Server.MapPath("~/Content/qr_" + nuevoQR + ".png");
                    qrImage.Save(qrPath, ImageFormat.Png);

                    // Enviar correo
                    var mail = new MailMessage();
                    mail.From = new MailAddress("tu_correo@gmail.com", "Cinemax");
                    mail.To.Add(email);
                    mail.Subject = "Confirmación de Reserva - Cinemax";
                    mail.Body = $"Hola {nombre},\n\nGracias por tu reserva.\n\n" +
                    $"Pelicula: {pelicula.PEL_Titulo}\n" +
                    $"Fecha y hora: {funcion.FUN_Fechahora}\n" +
                    $"Precio por entrada: ${funcion.FUN_Precio}\n" +
                    $"Cantidad de asientos: {asientos.Count}\n" +
                    $"Total pagado: ${funcion.FUN_Precio * asientos.Count}\n" +
                    $"Asientos: {string.Join(", ", asientosInfo)}\n" +
                    $"Código de Reserva: {nuevoQR}\n\n" +
                    "Adjunto encontrarás tu código QR.";




                    mail.Attachments.Add(new Attachment(qrPath));

                    var smtp = new SmtpClient("smtp.gmail.com", 587)
                    {
                        EnableSsl = true,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("luis2004eduardocristales@gmail.com", "uirq qfej xzgt nfot")
                    };
                    smtp.Send(mail);
                }

                return RedirectToAction("PagoAprovado");
            }
            catch (Exception ex)
            {
                TempData["ErrorPago"] = "Ocurrió un error: " + ex.Message;
                return RedirectToAction("Pago", new { id_funcion = TempData["id_Funcion"], id_usuario = TempData["id_Usuario"], asientos = TempData["Asientos"] });
            }
        }



        public ActionResult PagoAprovado()
        {
            return View();
        }
    }
}