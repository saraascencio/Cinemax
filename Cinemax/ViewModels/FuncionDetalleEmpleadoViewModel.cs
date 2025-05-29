using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cinemax.ViewModels
{
    public class FuncionDetalleEmpleadoViewModel
    {
        public int ID_Funcion { get; set; }
        public string PeliculaTitulo { get; set; }
        public string Hora { get; set; }
        public decimal Precio { get; set; }
        public int AsientosDisponibles { get; set; }
        public int AsientosOcupados { get; set; }
        public int TotalAsientos { get; set; }
    }
}