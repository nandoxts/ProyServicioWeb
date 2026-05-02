namespace ProyMvcProyectoOnline205.Models
{
    /// <summary>
    /// Helper centralizado para resolver la URL de imagen de un producto.
    /// Maneja 3 formatos historicos coexistentes en BD:
    ///   1) URL absoluta:    "https://..." o "//cdn..."   -> tal cual
    ///   2) Ruta web local:  "/uploads/productos/abc.jpg" -> tal cual
    ///   3) Formato legacy:  RutaImagen="img/" + NombreImagen="foo.jpg"
    ///                       -> "/img/foo.jpg"
    /// </summary>
    public static class ProductoImagen
    {
        public static string Resolver(string? rutaImagen, string? nombreImagen)
        {
            var ruta   = (rutaImagen   ?? "").Trim();
            var nombre = (nombreImagen ?? "").Trim();

            if (ruta.Length == 0 && nombre.Length == 0) return "";

            // 1) URL absoluta -> tal cual
            if (ruta.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) ||
                ruta.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase) ||
                ruta.StartsWith("//"))
            {
                return ruta;
            }

            // 2) Ruta local que ya empieza con / (ej. /uploads/productos/abc.jpg)
            if (ruta.StartsWith("/"))
            {
                // Si ya incluye un nombre de archivo (tiene extension), no concatenar
                if (System.IO.Path.HasExtension(ruta)) return ruta;
                // Caso raro: solo carpeta con / inicial
                if (nombre.Length > 0)
                    return ruta.TrimEnd('/') + "/" + nombre.TrimStart('/');
                return ruta;
            }

            // 3) Formato legacy: ruta es carpeta y nombre es archivo
            if (ruta.Length > 0 && nombre.Length > 0)
            {
                return "/" + ruta.TrimStart('/').TrimEnd('/') + "/" + nombre.TrimStart('/');
            }

            // 4) Solo hay nombre -> asume carpeta /img/
            if (nombre.Length > 0)
            {
                return "/img/" + nombre.TrimStart('/');
            }

            return "";
        }

        public static string Resolver(Producto? p)
            => p == null ? "" : Resolver(p.RutaImagen, p.NombreImagen);
    }
}
