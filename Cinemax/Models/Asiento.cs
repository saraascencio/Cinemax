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
    
    public partial class Asiento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Asiento()
        {
            this.FuncionAsiento = new HashSet<FuncionAsiento>();
            this.Boleto = new HashSet<Boleto>();
        }
    
        public int ID_Asiento { get; set; }
        public Nullable<int> ID_Sala { get; set; }
        public string ASI_Fila { get; set; }
        public int ASI_Numero { get; set; }
    
        public virtual Sala Sala { get; set; }
        public virtual Sala Sala1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FuncionAsiento> FuncionAsiento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Boleto> Boleto { get; set; }
    }
}
