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
    
    public partial class Reserva
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Reserva()
        {
            this.Boleto = new HashSet<Boleto>();
            this.Pago = new HashSet<Pago>();
            this.Pago1 = new HashSet<Pago>();
        }
    
        public int ID_Reserva { get; set; }
        public Nullable<int> ID_Funcion { get; set; }
        public Nullable<int> ID_Usuario { get; set; }
        public Nullable<int> ID_REstado { get; set; }
        public System.DateTime RES_Reserva { get; set; }
        public string RES_QR { get; set; }
        public string Nombre_Cliente { get; set; }
        public string Telefono_Cliente { get; set; }
        public string Email_Cliente { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Boleto> Boleto { get; set; }
        public virtual EstadoReserva EstadoReserva { get; set; }
        public virtual EstadoReserva EstadoReserva1 { get; set; }
        public virtual Funcion Funcion { get; set; }
        public virtual Funcion Funcion1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pago> Pago { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pago> Pago1 { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
