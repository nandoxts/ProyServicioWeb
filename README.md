# ProyServicioWeb — Sistema de Tienda Online 2025

Proyecto académico de tienda online compuesto por dos aplicaciones .NET 8:

- **API REST** (`SlnApiProyectoOnline2025`) — Backend con Entity Framework Core y SignalR para notificaciones en tiempo real.
- **MVC Web** (`SlnMvcProyectoOnline205`) — Frontend en ASP.NET Core MVC que consume la API.

La base de datos es **SQL Server** (`proyectodiciembre2025`) y se levanta a partir de tres scripts SQL incluidos en la raíz del repositorio.

---

## 1. Requisitos previos

Antes de levantar el proyecto asegúrate de tener instalado:

| Herramienta | Versión recomendada | Notas |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **8.0** o superior | Requerido por ambos proyectos (`<TargetFramework>net8.0</TargetFramework>`) |
| [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) | 2019 / 2022 | Por defecto el proyecto apunta a la instancia `localhost\MSSQLSERVER2022` |
| [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) | Última | Para ejecutar los scripts SQL |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/) | 17.8+ | Con la carga de trabajo *ASP.NET y desarrollo web* |
| [wkhtmltopdf](https://wkhtmltopdf.org/downloads.html) | 0.12.x | Necesario para `Rotativa.AspNetCore` (generación de PDFs) |
| Git | Cualquiera reciente | Para clonar el repositorio |

---

## 2. Estructura del repositorio

```
ProyServicioWeb/
├── base de datos para proyecto 2025 parte 2.sql        # Script de creación de la BD y tablas
├── insertar datos.sql                                  # Script de datos iniciales
├── Procedimiento alamacenados ... parte 3 ...sql       # Stored procedures
├── SlnApiProyectoOnline2025/                           # Solución de la API
│   └── ProyApiProyectoOnline2025/
│       ├── Controllers/   # Categoria, Cliente, Marca, Pedido, Producto, Ticket, Usuario, Notificacion
│       ├── Models/        # Entidades EF + Proyectodiciembre2025Context
│       ├── Hubs/          # NotificacionHub (SignalR)
│       └── Program.cs
└── SlnMvcProyectoOnline205/                            # Solución del frontend MVC
    └── ProyMvcProyectoOnline205/
        ├── Controllers/   # Autenticacion, Carrito, Cliente, Dashboard, Pedido, Producto, Ticket, Usuario
        ├── Views/
        ├── ViewComponents/
        ├── wwwroot/       # Aquí va Rotativa/wkhtmltopdf.exe
        └── Program.cs
```

---

## 3. Clonar el repositorio

```bash
git clone https://github.com/nandoxts/ProyServicioWeb.git
cd ProyServicioWeb
```

---

## 4. Configurar la base de datos

Los scripts deben ejecutarse **en este orden** sobre tu instancia de SQL Server:

1. `base de datos para proyecto 2025 parte 2.sql` — crea la base `proyectodiciembre2025` y todas las tablas.
2. `insertar datos.sql` — inserta datos iniciales (categorías, marcas, productos, ubicaciones, usuarios, etc.).
3. `Procedimiento alamacenados para el proyecto 2025 parte 3 actualizadosql.sql` — crea los procedimientos almacenados.

### Opción A — Desde SSMS

1. Abre SSMS y conéctate a tu servidor (`localhost\MSSQLSERVER2022` o el que uses).
2. Abre cada `.sql` en el orden indicado y presiona **F5** para ejecutarlo.

### Opción B — Desde la línea de comandos (`sqlcmd`)

```powershell
sqlcmd -S localhost\MSSQLSERVER2022 -U sa -P sql -i "base de datos para proyecto 2025 parte 2.sql"
sqlcmd -S localhost\MSSQLSERVER2022 -U sa -P sql -i "insertar datos.sql"
sqlcmd -S localhost\MSSQLSERVER2022 -U sa -P sql -i "Procedimiento alamacenados para el proyecto 2025 parte 3 actualizadosql.sql"
```

### Cadenas de conexión

Si tu instancia, usuario o contraseña son distintos a los valores por defecto, edita las cadenas de conexión:

- API → [SlnApiProyectoOnline2025/ProyApiProyectoOnline2025/appsettings.json](SlnApiProyectoOnline2025/ProyApiProyectoOnline2025/appsettings.json)
  ```json
  "ConnectionStrings": {
    "cn1": "Server=localhost\\MSSQLSERVER2022;Database=proyectodiciembre2025;User Id=sa;Password=sql;Encrypt=False;TrustServerCertificate=True;"
  }
  ```
- MVC → [SlnMvcProyectoOnline205/ProyMvcProyectoOnline205/appsettings.json](SlnMvcProyectoOnline205/ProyMvcProyectoOnline205/appsettings.json)
  ```json
  "ConnectionStrings": {
    "cn1": "server=localhost\\MSSQLSERVER2022;database=proyectodiciembre2025;uid=sa;pwd=sql;"
  }
  ```

---

## 5. Configurar Rotativa (generación de PDFs)

El proyecto MVC usa `Rotativa.AspNetCore` para imprimir reportes/boletas en PDF. Necesita el ejecutable `wkhtmltopdf.exe`:

1. Descarga **wkhtmltopdf** desde [wkhtmltopdf.org](https://wkhtmltopdf.org/downloads.html).
2. Crea la carpeta `wwwroot/Rotativa/` dentro de `ProyMvcProyectoOnline205` si no existe.
3. Copia los binarios (`wkhtmltopdf.exe`, `wkhtmltoimage.exe` y dependencias) en esa carpeta.

> El nombre `Rotativa` se referencia en [Program.cs](SlnMvcProyectoOnline205/ProyMvcProyectoOnline205/Program.cs) mediante `RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa")`.

---

## 6. Restaurar paquetes NuGet

Desde la raíz del repositorio:

```powershell
dotnet restore SlnApiProyectoOnline2025\SlnApiProyectoOnline2025.sln
dotnet restore SlnMvcProyectoOnline205\SlnMvcProyectoOnline205.sln
```

---

## 7. Levantar el proyecto

> **Importante:** debes levantar **primero la API** y luego el MVC. El frontend depende de la API en `http://localhost:5064`.

### Puertos por defecto

| Aplicación | URL |
|---|---|
| API REST + Swagger | http://localhost:5064 (Swagger en `/swagger`) |
| Hub SignalR | http://localhost:5064/notificacionHub |
| MVC Web | http://localhost:5140 |

La política CORS de la API solo permite el origen `http://localhost:5140`, así que respeta esos puertos o ajusta ambos lados.

### Opción A — Visual Studio 2022

1. Abre **dos instancias** de Visual Studio (o usa la opción *Múltiples proyectos de inicio*).
2. En la primera abre `SlnApiProyectoOnline2025.sln` → ejecuta con **F5** o **Ctrl+F5**.
3. En la segunda abre `SlnMvcProyectoOnline205.sln` → ejecuta con **F5** o **Ctrl+F5**.

### Opción B — Línea de comandos (recomendada para verificar)

Terminal 1 — API:

```powershell
cd SlnApiProyectoOnline2025\ProyApiProyectoOnline2025
dotnet run
```

Terminal 2 — MVC:

```powershell
cd SlnMvcProyectoOnline205\ProyMvcProyectoOnline205
dotnet run
```

Abre el navegador en http://localhost:5140 para usar la tienda y en http://localhost:5064/swagger para probar la API.

---

## 8. Funcionalidades principales

- Autenticación de **clientes** y **usuarios/admin** (sesión en MVC con `IdleTimeout = 30 min`).
- Catálogo de productos con **categorías**, **marcas** e **imágenes**.
- **Carrito** y registro de **pedidos** con detalle de venta y pagos.
- Gestión de **direcciones** (Departamento → Provincia → Distrito).
- Sistema de **tickets** de soporte con bitácora (`TicketLog`).
- **Notificaciones en tiempo real** vía SignalR (`/notificacionHub`) tanto para administradores como para clientes.
- Generación de **reportes en PDF** vía Rotativa.
- **Dashboard** administrativo.

---

## 9. Solución de problemas frecuentes

| Síntoma | Causa probable | Solución |
|---|---|---|
| `Cannot open database "proyectodiciembre2025"` | Scripts no ejecutados o instancia equivocada | Ejecuta los 3 scripts en orden y revisa la cadena de conexión |
| `Login failed for user 'sa'` | Autenticación SQL deshabilitada o contraseña distinta | Habilita *SQL Server Authentication* y actualiza `appsettings.json` |
| MVC carga pero no llega a la API | API apagada o puerto distinto | Verifica que la API esté en `http://localhost:5064` |
| CORS bloqueado | El MVC corre en puerto distinto a `5140` | Ajusta `WithOrigins(...)` en [Program.cs](SlnApiProyectoOnline2025/ProyApiProyectoOnline2025/Program.cs) o cambia el puerto del MVC |
| Error al generar PDF | Falta `wkhtmltopdf.exe` en `wwwroot/Rotativa/` | Copia los binarios como se indica en el paso 5 |
| Notificaciones no llegan | Hub no conecta | Confirma que la API esté arriba y que el cliente apunte a `/notificacionHub` |

---

## 10. Comandos útiles

```powershell
# Compilar ambas soluciones
dotnet build SlnApiProyectoOnline2025\SlnApiProyectoOnline2025.sln
dotnet build SlnMvcProyectoOnline205\SlnMvcProyectoOnline205.sln

# Limpiar binarios
dotnet clean SlnApiProyectoOnline2025\SlnApiProyectoOnline2025.sln
dotnet clean SlnMvcProyectoOnline205\SlnMvcProyectoOnline205.sln
```

---

## 11. Autor

Repositorio: [github.com/nandoxts/ProyServicioWeb](https://github.com/nandoxts/ProyServicioWeb)
