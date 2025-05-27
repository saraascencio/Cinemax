using Cinemax.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace Cinemax.ViewModels
{
    public class HistorialReservaViewModel
    {
      
            public int Id { get; set; }
            public string Fecha { get; set; }
            public string Pelicula { get; set; }
            public decimal Precio { get; set; }
            public string Sala { get; set; }
            public string Asientos { get; set; }
            public string CodigoQR { get; set; }
            public string Estado { get; set; }
        
    }
}