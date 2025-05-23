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
    
    public partial class Sala
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sala()
        {
            this.Asiento = new HashSet<Asiento>();
            this.Asiento1 = new HashSet<Asiento>();
            this.Funcion = new HashSet<Funcion>();
            this.Funcion1 = new HashSet<Funcion>();
        }
    
        public int ID_Sala { get; set; }
        public Nullable<int> SAL_Tipo { get; set; }
        public Nullable<int> ID_Sucursal { get; set; }
        public string SAL_Nombre { get; set; }
        public int SAL_Capacidad { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Asiento> Asiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Asiento> Asiento1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Funcion> Funcion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Funcion> Funcion1 { get; set; }
        public virtual Sucursal Sucursal { get; set; }
        public virtual Sucursal Sucursal1 { get; set; }
        public virtual TipoSala TipoSala { get; set; }
        public virtual TipoSala TipoSala1 { get; set; }
    }
}
