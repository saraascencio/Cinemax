using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.ViewModels
{
    public class FuncionesEmpleadoViewModel
    {
        public string PeliculaBusqueda { get; set; }
        public IEnumerable<SelectListItem> Peliculas { get; set; }
        public List<FuncionSalaEViewModel> FuncionesPorSala { get; set; }
        public DateTime FechaConsulta { get; set; }
    }
}