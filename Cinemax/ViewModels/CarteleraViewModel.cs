using Cinemax.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace Cinemax.ViewModels
{
    public class CarteleraViewModel
    {
         public List<Funcion> Funciones { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }
}