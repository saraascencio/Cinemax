using Cinemax.Models;
using Cinemax.Servicios;
using Cinemax.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace Cinemax.Controllers
{
    public class EmpleadoFuncionController : Controller
    {

        private CinemaxEntities _dbContext;

        public EmpleadoFuncionController()
        {
            _dbContext = new CinemaxEntities();
        }

        [Autenticacion]
        public ActionResult FuncionesEmpleado(string peliculaBusqueda = "")
        {
            var hoy = DateTime.Today;

           
            var peliculas = _dbContext.Pelicula
                .OrderBy(p => p.PEL_Titulo)
                .Select(p => new SelectListItem
                {
                    Value = p.PEL_Titulo,
                    Text = p.PEL_Titulo,
                    Selected = p.PEL_Titulo == peliculaBusqueda
                })
                .ToList();

            peliculas.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Todas las películas",
                Selected = string.IsNullOrEmpty(peliculaBusqueda)
            });

         
            var funcionesQuery = _dbContext.Funcion
                .Include("Pelicula") 
                .Include("Sala")      
                .Where(f => DbFunctions.TruncateTime(f.FUN_Fechahora) == hoy);

      
            if (!string.IsNullOrEmpty(peliculaBusqueda))
            {
                funcionesQuery = funcionesQuery.Where(f => f.Pelicula.PEL_Titulo.Contains(peliculaBusqueda));
            }

          
            var funciones = funcionesQuery
                .OrderBy(f => f.Sala.SAL_Nombre)
                .ThenBy(f => f.FUN_Fechahora)
                .ToList();  

            
            var funcionesPorSala = funciones
                .GroupBy(f => new { f.Sala.ID_Sala, f.Sala.SAL_Nombre })
                .Select(grupo => new FuncionSalaEViewModel
                {
                    SalaId = grupo.Key.ID_Sala,
                    SalaNombre = grupo.Key.SAL_Nombre,
                    Funciones = grupo.Select(f => new FuncionDetalleEmpleadoViewModel
                    {
                        ID_Funcion = f.ID_Funcion,
                        PeliculaTitulo = f.Pelicula.PEL_Titulo,
                        Hora = f.FUN_Fechahora.ToString("HH:mm"),  
                        Precio = f.FUN_Precio,
                        AsientosDisponibles = ObtenerCantidadAsientosDisponibles(f.ID_Funcion),
                        AsientosOcupados = ObtenerCantidadAsientosOcupados(f.ID_Funcion),
                        TotalAsientos = ObtenerTotalAsientosSala(f.ID_Sala ?? 0)
                    }).ToList()
                })
                .ToList();

            var model = new FuncionesEmpleadoViewModel
            {
                PeliculaBusqueda = peliculaBusqueda,
                Peliculas = peliculas,
                FuncionesPorSala = funcionesPorSala,
                FechaConsulta = hoy
            };

            return View(model);
        }


        [Autenticacion]
        [HttpPost]
        public ActionResult BuscarFunciones(string peliculaBusqueda)
        {
            return RedirectToAction("FuncionesEmpleado", new { peliculaBusqueda = peliculaBusqueda });
        }

        private int ObtenerCantidadAsientosDisponibles(int idFuncion)
        {
            var asientosOcupadosIds = _dbContext.Boleto
                .Where(b => b.Reserva.ID_Funcion == idFuncion && b.ID_Asiento != null)
                .Select(b => b.ID_Asiento.Value)
                .Distinct()
                .ToList();

            var funcionConSala = _dbContext.Funcion
                .Include("Sala")
                .FirstOrDefault(f => f.ID_Funcion == idFuncion);

            if (funcionConSala?.Sala == null) return 0;

            var totalAsientos = _dbContext.Asiento
                .Where(a => a.ID_Sala == funcionConSala.Sala.ID_Sala)
                .Count();

            return totalAsientos - asientosOcupadosIds.Count;
        }

        private int ObtenerCantidadAsientosOcupados(int idFuncion)
        {
            return _dbContext.Boleto
                .Where(b => b.Reserva.ID_Funcion == idFuncion && b.ID_Asiento != null)
                .Select(b => b.ID_Asiento.Value)
                .Distinct()
                .Count();
        }

        private int ObtenerTotalAsientosSala(int idSala)
        {
            return _dbContext.Asiento
                .Where(a => a.ID_Sala == idSala)
                .Count();
        }
    }
}