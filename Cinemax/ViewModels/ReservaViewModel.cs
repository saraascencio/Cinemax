using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cinemax.ViewModels
{
    public class ReservaViewModel
    {
        public int ID_Reserva { get; set; }
        public DateTime FuncionFecha { get; set; }
        public string PeliculaNombre { get; set; }
        public decimal Precio { get; set; }
        public string SalaNombre { get; set; }
        public string Asientos { get; set; }
        public string QR { get; set; }
        public string QRTexto { get; set; } // Texto QR original (ej: "QR123456")
        public string QRImagen { get; set; } // Imagen QR en base64
        public string Estado { get; set; }
    }
}