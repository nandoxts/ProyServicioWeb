using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class Distrito
{
    public string IdDistrito { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string IdProvincia { get; set; } = null!;

    public string IdDepartamento { get; set; } = null!;

    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;

    public virtual Provincium IdProvinciaNavigation { get; set; } = null!;

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
