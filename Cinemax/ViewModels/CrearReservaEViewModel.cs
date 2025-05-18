using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.ViewModels
{
    public class CrearReservaEViewModel
    {
     
        [Display(Name = "Email del cliente")]
        public string EmailCliente { get; set; }

        public bool EsClienteRegistrado { get; set; }

        [Display(Name = "Nombre del cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Email registrado")]

        public string EmailCliente2 { get; set; }

        [Display(Name = "Teléfono del cliente")]
        public string TelefonoCliente { get; set; }


        public int? UsuarioId { get; set; }

        
        [Display(Name = "Película")]
        [Required(ErrorMessage = "Debe seleccionar una película")]
        public int? PeliculaId { get; set; }
        public IEnumerable<SelectListItem> Peliculas { get; set; }

        
        [Display(Name = "Fecha")]
        public string FechaSeleccionada { get; set; }

        [Display(Name = "Hora")]
        [Required(ErrorMessage = "Debe seleccionar una hora")]
        public int? FuncionId { get; set; }

        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Display(Name = "Sala")]
        public string NombreSala { get; set; }

        
        [Display(Name = "Asientos seleccionados")]
        [Required(ErrorMessage = "Debe seleccionar al menos un asiento")]
        public string AsientosSeleccionados { get; set; }

      
        [Display(Name = "Método de pago")]
        [Required(ErrorMessage = "Debe seleccionar un método de pago")]
        public int MetodoPagoId { get; set; }
        public IEnumerable<SelectListItem> MetodosPago { get; set; }

        public CrearReservaEViewModel()
        {
            Peliculas = new List<SelectListItem>();
            MetodosPago = new List<SelectListItem>();
        }
    }
}