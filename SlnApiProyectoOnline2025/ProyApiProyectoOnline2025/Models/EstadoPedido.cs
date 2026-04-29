using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyApiProyectoOnline2025.Models
{
    [Table("Estado_Pedido")]
    public partial class EstadoPedido
    {
        [Key]
        public int IdEstadoPedido { get; set; }

        public string Descripcion { get; set; } = null!;

        // 👇 relación CORRECTA (uno a muchos)
        public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
    }

}
