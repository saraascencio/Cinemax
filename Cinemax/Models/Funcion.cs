//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Funcion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Funcion()
        {
            this.FuncionAsiento = new HashSet<FuncionAsiento>();
            this.FuncionAsiento1 = new HashSet<FuncionAsiento>();
            this.Reserva = new HashSet<Reserva>();
            this.Reserva1 = new HashSet<Reserva>();
        }
    
        public int ID_Funcion { get; set; }
        public Nullable<int> ID_Pelicula { get; set; }
        public Nullable<int> ID_Sala { get; set; }
        public System.DateTime FUN_Fechahora { get; set; }
        public decimal FUN_Precio { get; set; }
    
        public virtual Pelicula Pelicula { get; set; }
        public virtual Pelicula Pelicula1 { get; set; }
        public virtual Sala Sala { get; set; }
        public virtual Sala Sala1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FuncionAsiento> FuncionAsiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FuncionAsiento> FuncionAsiento1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Reserva> Reserva { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Reserva> Reserva1 { get; set; }
    }
}
