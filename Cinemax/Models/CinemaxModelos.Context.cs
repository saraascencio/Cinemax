﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cinemax.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CinemaxEntities : DbContext
    {
        public CinemaxEntities()
            : base("name=CinemaxEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Asiento> Asiento { get; set; }
        public virtual DbSet<Boleto> Boleto { get; set; }
        public virtual DbSet<EstadoAsiento> EstadoAsiento { get; set; }
        public virtual DbSet<EstadoPago> EstadoPago { get; set; }
        public virtual DbSet<EstadoReserva> EstadoReserva { get; set; }
        public virtual DbSet<Funcion> Funcion { get; set; }
        public virtual DbSet<FuncionAsiento> FuncionAsiento { get; set; }
        public virtual DbSet<Genero> Genero { get; set; }
        public virtual DbSet<MetodoPago> MetodoPago { get; set; }
        public virtual DbSet<Pago> Pago { get; set; }
        public virtual DbSet<Pelicula> Pelicula { get; set; }
        public virtual DbSet<Reserva> Reserva { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<Sala> Sala { get; set; }
        public virtual DbSet<Sucursal> Sucursal { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<TipoSala> TipoSala { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
    }
}
