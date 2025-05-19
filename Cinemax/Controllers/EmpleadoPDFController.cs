using Cinemax.Models;
using Cinemax.Servicios;
using Cinemax.ViewModels;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.Controllers
{
    public class EmpleadoPDFController : Controller
    {

        private CinemaxEntities _dbContext;

        public EmpleadoPDFController()
        {
            _dbContext = new CinemaxEntities();
        }

        [Autenticacion]

        public ActionResult Detalles(int id)
        {
            var reservaAgrupada = ObtenerDatosReserva(id);
            if (reservaAgrupada == null)
            {
                return HttpNotFound();
            }
            return View(reservaAgrupada);
        }

        // Acción solo para PDF
        public ActionResult DetallesPDF(int id)
        {
            var reservaAgrupada = ObtenerDatosReserva(id);
            if (reservaAgrupada == null)
            {
                return HttpNotFound();
            }

            string fechaHoy = DateTime.Now.ToString("yyyyMMdd-HHmm");
            return new Rotativa.ViewAsPdf("DetallesPDF", reservaAgrupada)
            {

                FileName = $"Reserva-{id}-{fechaHoy}.pdf",

                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type"
            };
        }

        // Método privado para reutilizar la lógica
        private ReservaViewModel ObtenerDatosReserva(int id)
        {
            var consultaDetalle = (from r in _dbContext.Reserva
                                   join f in _dbContext.Funcion on r.ID_Funcion equals f.ID_Funcion
                                   join p in _dbContext.Pelicula on f.ID_Pelicula equals p.ID_Pelicula
                                   join s in _dbContext.Sala on f.ID_Sala equals s.ID_Sala
                                   join er in _dbContext.EstadoReserva on r.ID_REstado equals er.ID_Estadoreserva
                                   join b in _dbContext.Boleto on r.ID_Reserva equals b.ID_Reserva into boletos
                                   from b in boletos.DefaultIfEmpty()
                                   join a in _dbContext.Asiento on (b == null ? null : (int?)b.ID_Asiento) equals a.ID_Asiento into asientos
                                   from a in asientos.DefaultIfEmpty()
                                   where r.ID_Reserva == id
                                   select new
                                   {
                                       r.ID_Reserva,
                                       p.PEL_Titulo,
                                       f.FUN_Fechahora,
                                       f.FUN_Precio,
                                       s.SAL_Nombre,
                                       AsientoFila = a != null ? a.ASI_Fila : null,
                                       AsientoNumero = a != null ? (int?)a.ASI_Numero : null,
                                       r.RES_QR,
                                       er.ESR_Estado
                                   }).ToList();

            var reservaAgrupada = consultaDetalle
                .GroupBy(x => x.ID_Reserva)
                .Select(g => new ReservaViewModel
                {
                    ID_Reserva = g.Key,
                    PeliculaNombre = g.First().PEL_Titulo,
                    FuncionFecha = g.First().FUN_Fechahora,
                    Precio = g.First().FUN_Precio,
                    SalaNombre = g.First().SAL_Nombre,
                    Asientos = string.Join(",", g
                        .Where(x => x.AsientoFila != null && x.AsientoNumero != null)
                        .Select(x => $"{x.AsientoFila}{x.AsientoNumero}")
                        .Distinct()),
                    QRTexto = g.First().RES_QR, // Guardamos el texto QR original
                    QRImagen = GenerarQRCode(g.First().RES_QR), // Generamos la imagen
                    Estado = g.First().ESR_Estado
                }).FirstOrDefault();

            return reservaAgrupada;
        }

        private string GenerarQRCode(string textoQR)
        {
            if (string.IsNullOrEmpty(textoQR))
                return null;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQR, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
            }
        }
    }
}