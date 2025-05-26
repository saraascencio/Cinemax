using Cinemax.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.ViewModels
{
    public  class FuncionDetalleViewModel
    {
        public Funcion Funcion { get; set; }
        public List<FuncionAsiento> Asientos { get; set; }
    }
}