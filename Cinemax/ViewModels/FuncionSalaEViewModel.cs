using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cinemax.ViewModels
{
    public class FuncionSalaEViewModel
    {
        public int SalaId { get; set; }
        public string SalaNombre { get; set; }
        public List<FuncionDetalleEmpleadoViewModel> Funciones { get; set; }
    }
}