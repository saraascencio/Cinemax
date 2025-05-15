using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cinemax.ViewModels
{

    public class EditarReservaViewModel
    {
        // Identificador
        public int ID_Reserva { get; set; }

        // Película
        public int ID_Pelicula { get; set; }
        public string PeliculaNombre { get; set; }
        public IEnumerable<SelectListItem> Peliculas { get; set; }

        // Función
        public int ID_Funcion { get; set; }
        public string FuncionFecha { get; set; }
        public DateTime? FechaSeleccionada { get; set; }
        public string HoraSeleccionada { get; set; }
        public IEnumerable<SelectListItem> HorasDisponibles { get; set; }
       

        public IEnumerable<SelectListItem> Funciones { get; set; }

        // Información de la función
        public decimal Precio { get; set; }
        public string SalaNombre { get; set; }

        // Asientos (nueva estructura)
        [Display(Name = "Asientos Disponibles")]
        public List<string> AsientosDisponiblesLista { get; set; } // Lista simple de strings

        [Display(Name = "Asientos Seleccionados")]
        public List<string> AsientosSeleccionados { get; set; } = new List<string>();

        public string AsientosSeleccionadosTexto
        {
            get => string.Join(",", AsientosSeleccionados);
            set => AsientosSeleccionados = value?.Split(',').ToList() ?? new List<string>();
        }
        [Display(Name = "Seleccionar Asientos")]
        public MultiSelectList AsientosDisponibles { get; set; } // Se mantiene por compatibilidad

        // Datos de reserva
        public string QR { get; set; }

        // Estado
        public string Estado { get; set; }
        public string EstadoNombre { get; set; }
        public IEnumerable<SelectListItem> Estados { get; set; }

        // Control de cambios
        public bool PeliculaCambiada { get; set; }
        public bool FuncionCambiada { get; set; }
        public string NuevoAsiento { get; set; }    // Para agregar asientos
        public string AsientoAEliminar { get; set; }

     
    }
}