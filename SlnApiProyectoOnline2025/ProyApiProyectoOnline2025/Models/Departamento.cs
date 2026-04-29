using System;
using System.Collections.Generic;

namespace ProyApiProyectoOnline2025.Models;

public partial class Departamento
{
    public string IdDepartamento { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Distrito> Distritos { get; set; } = new List<Distrito>();

    public virtual ICollection<Provincium> Provincia { get; set; } = new List<Provincium>();
}
