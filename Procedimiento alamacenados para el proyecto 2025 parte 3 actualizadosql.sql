USE proyectodiciembre2025
GO
--------------------------------------------------------------------------------
-- 1. REPORTES Y ESTADISTICAS
--------------------------------------------------------------------------------

-- Resumen de Ventas Agrupadas por Mes y A o
CREATE OR ALTER PROCEDURE ssp_ResumenVentasPorMes
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SELECT
        YEAR(v.FechaVenta) AS Anio,
        MONTH(v.FechaVenta) AS Mes,
        COUNT(DISTINCT v.IdVenta) AS NumeroVentas,
        SUM(dv.Cantidad) AS TotalItemsVendidos,
        SUM(dv.Total) AS MontoTotal
    FROM VENTA v
    INNER JOIN DETALLE_VENTA dv ON dv.IdVenta = v.IdVenta
    WHERE v.FechaVenta BETWEEN @StartDate AND @EndDate
    GROUP BY YEAR(v.FechaVenta), MONTH(v.FechaVenta)
    ORDER BY Anio, Mes;
END;
GO

-- Historial Completo de Compras de un Cliente (Devuelve dos conjuntos de resultados: Venta y Detalles)
CREATE OR ALTER PROCEDURE ssp_HistorialCliente
    @IdCliente INT
AS
BEGIN
    SELECT v.IdVenta, v.FechaVenta, v.MontoTotal, v.Direccion, v.Telefono
    FROM VENTA v
    WHERE v.IdCliente = @IdCliente
    ORDER BY v.FechaVenta DESC;

    SELECT dv.IdDetalleVenta, dv.IdVenta, dv.IdProducto, p.Nombre,
           dv.Cantidad, dv.Total
    FROM DETALLE_VENTA dv
    INNER JOIN PRODUCTO p ON p.IdProducto = dv.IdProducto
    WHERE dv.IdVenta IN (
        SELECT IdVenta FROM VENTA WHERE IdCliente = @IdCliente
    );
END;
GO


-- Resumen de Ventas por Empleado/Usuario en un Rango de Fechas
CREATE OR ALTER PROCEDURE ssp_VentasPorEmpleado
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SELECT u.IdUsuario, u.Nombres, u.Apellidos,
           COUNT(DISTINCT v.IdVenta) AS CantidadVentas,
           SUM(dv.Total) AS MontoTotal
    FROM VENTA v
    INNER JOIN DETALLE_VENTA dv ON dv.IdVenta = v.IdVenta
    INNER JOIN USUARIO u ON u.IdUsuario = v.IdUsuarioEmpleado
    WHERE v.FechaVenta BETWEEN @StartDate AND @EndDate
    GROUP BY u.IdUsuario, u.Nombres, u.Apellidos
    ORDER BY MontoTotal DESC;
END;
GO


--------------------------------------------------------------------------------
-- 2. CAT LOGO: ROL (CRUD B sico)
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE ssp_Rol_Listar
AS
BEGIN
    SELECT * FROM ROL ORDER BY Descripcion;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Rol_Obtener
@IdRol INT
AS
BEGIN
    SELECT * FROM ROL WHERE IdRol = @IdRol;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Rol_Insertar
@Descripcion VARCHAR(50)
AS
BEGIN
    INSERT INTO ROL (Descripcion) VALUES (@Descripcion);
    SELECT SCOPE_IDENTITY() AS IdRol;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Rol_Actualizar
@IdRol INT,
@Descripcion VARCHAR(50)
AS
BEGIN
    UPDATE ROL SET Descripcion = @Descripcion WHERE IdRol = @IdRol;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Rol_Eliminar
@IdRol INT
AS
BEGIN
    DELETE FROM ROL WHERE IdRol = @IdRol;
END;
GO


--------------------------------------------------------------------------------
-- 3. CAT LOGO: CATEGOR A (CRUD B sico)
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE ssp_Categoria_Listar
AS
BEGIN
    SELECT * FROM CATEGORIA ORDER BY Descripcion;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Categoria_Obtener
@IdCategoria INT
AS
BEGIN
    SELECT * FROM CATEGORIA WHERE IdCategoria = @IdCategoria;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Categoria_Insertar
@Descripcion VARCHAR(100)
AS
BEGIN
    INSERT INTO CATEGORIA (Descripcion) VALUES (@Descripcion);
    SELECT SCOPE_IDENTITY() AS IdCategoria;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Categoria_Actualizar
@IdCategoria INT,
@Descripcion VARCHAR(100)
AS
BEGIN
    UPDATE CATEGORIA
    SET Descripcion = @Descripcion
    WHERE IdCategoria = @IdCategoria;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Categoria_Eliminar
@IdCategoria INT
AS
BEGIN
    DELETE FROM CATEGORIA WHERE IdCategoria = @IdCategoria;
END;
GO
--------------------------------------------------------------------------------
-- 4. CAT LOGO: MARCA (CRUD B sico)
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE ssp_Marca_Listar
AS
BEGIN
    SELECT * FROM MARCA ORDER BY Descripcion;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Marca_Obtener
@IdMarca INT
AS
BEGIN
    SELECT * FROM MARCA WHERE IdMarca = @IdMarca;
END;
GO


CREATE OR ALTER PROCEDURE ssp_Marca_Insertar
@Descripcion VARCHAR(100)
AS
BEGIN
    INSERT INTO MARCA (Descripcion) VALUES (@Descripcion);
    SELECT SCOPE_IDENTITY() AS IdMarca;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Marca_Actualizar
@IdMarca INT,
@Descripcion VARCHAR(100)
AS
BEGIN
    UPDATE MARCA
    SET Descripcion = @Descripcion
    WHERE IdMarca = @IdMarca;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Marca_Eliminar
@IdMarca INT
AS
BEGIN
    DELETE FROM MARCA WHERE IdMarca = @IdMarca;
END;
GO


--------------------------------------------------------------------------------
-- 5. ADMINISTRACI N: USUARIO (CRUD + Seguridad)
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE ssp_Usuario_Listar
AS
BEGIN
    SELECT u.*, r.Descripcion AS Rol
    FROM USUARIO u
    INNER JOIN ROL r ON u.IdRol = r.IdRol
    ORDER BY u.Nombres;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Usuario_Obtener
@IdUsuario INT
AS
BEGIN
    SELECT * FROM USUARIO WHERE IdUsuario = @IdUsuario;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Usuario_Insertar
@IdRol INT,
@Nombres VARCHAR(100),
@Apellidos VARCHAR(100),
@Correo VARCHAR(100),
@PasswordHash VARCHAR(512)
AS
BEGIN
    INSERT INTO USUARIO
    (IdRol, Nombres, Apellidos, Correo, PasswordHash, Activo, Reestablecer)
    VALUES (@IdRol, @Nombres, @Apellidos, @Correo, @PasswordHash, 1, 1);

    SELECT SCOPE_IDENTITY() AS IdUsuario;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Usuario_Actualizar
@IdUsuario INT,
@IdRol INT,
@Nombres VARCHAR(100),
@Apellidos VARCHAR(100),
@Correo VARCHAR(100),
@PasswordHash VARCHAR(512),
@Activo BIT
AS
BEGIN
    UPDATE USUARIO
    SET IdRol = @IdRol,
        Nombres = @Nombres,
        Apellidos = @Apellidos,
        Correo = @Correo,
        PasswordHash = COALESCE(NULLIF(@PasswordHash, ''), PasswordHash),
        Activo = @Activo
    WHERE IdUsuario = @IdUsuario;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Usuario_Eliminar
@IdUsuario INT
AS
BEGIN
    DELETE FROM USUARIO WHERE IdUsuario = @IdUsuario;
END;
GO


--------------------------------------------------------------------------------
-- 6. ADMINISTRACI N: CLIENTE (CRUD + Seguridad)
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE ssp_Cliente_Listar
AS
BEGIN
    SELECT * FROM CLIENTE ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Cliente_Obtener
@IdCliente INT
AS
BEGIN
    SELECT * FROM CLIENTE WHERE IdCliente = @IdCliente;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Cliente_Insertar
@Nombre VARCHAR(100),
@Apellidos VARCHAR(100),
@Correo VARCHAR(100),
@PasswordHash VARCHAR(512),
@Telefono VARCHAR(50)
AS
BEGIN
    INSERT INTO CLIENTE
    (Nombre, Apellidos, Correo, PasswordHash, Telefono, Activo, Reestablecer)
    VALUES (@Nombre, @Apellidos, @Correo, @PasswordHash, @Telefono, 1, 0);

    SELECT SCOPE_IDENTITY() AS IdCliente;
END;
GO

CREATE OR ALTER PROCEDURE ssp_Cliente_Actualizar
@IdCliente INT,
@Nombre VARCHAR(100),
@Apellidos VARCHAR(100),
@Correo VARCHAR(100),
@PasswordHash VARCHAR(512),
@Telefono VARCHAR(50),
@Activo BIT
AS
BEGIN
    UPDATE CLIENTE
    SET Nombre = @Nombre,
        Apellidos = @Apellidos,
        Correo = @Correo,
        Telefono = @Telefono,
        Activo = @Activo,
        PasswordHash = COALESCE(NULLIF(@PasswordHash, ''), PasswordHash)
    WHERE IdCliente = @IdCliente;
END;
GO
--------------------------------------------------------------------------------
-- 7. INVENTARIO: PRODUCTO (CRUD + Filtros)
--------------------------------------------------------------------------------

-- Insertar Producto
CREATE OR ALTER PROCEDURE ssp_Producto_Insertar
@Nombre VARCHAR(500),
@Descripcion VARCHAR(500),
@IdMarca INT,
@IdCategoria INT,
@Precio DECIMAL(10,2),
@Stock INT,
@RutaImagen VARCHAR(100),
@NombreImagen VARCHAR(100)
AS
BEGIN
    INSERT INTO PRODUCTO
    (Nombre, Descripcion, IdMarca, IdCategoria, Precio, Stock,
     RutaImagen, NombreImagen, Activo)
    VALUES
    (@Nombre, @Descripcion, @IdMarca, @IdCategoria, @Precio,
     @Stock, @RutaImagen, @NombreImagen, 1);

    SELECT SCOPE_IDENTITY() AS IdProducto;
END;
GO


-- Actualizar Producto
CREATE OR ALTER PROCEDURE ssp_Producto_Actualizar
@IdProducto INT,
@Nombre VARCHAR(500),
@Descripcion VARCHAR(500),
@IdMarca INT,
@IdCategoria INT,
@Precio DECIMAL(10,2),
@Stock INT,
@RutaImagen VARCHAR(100),
@NombreImagen VARCHAR(100),
@Activo BIT
AS
BEGIN
    UPDATE PRODUCTO
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        IdMarca = @IdMarca,
        IdCategoria = @IdCategoria,
        Precio = @Precio,
        Stock = @Stock,
        RutaImagen = @RutaImagen,
        NombreImagen = @NombreImagen,
        Activo = @Activo
    WHERE IdProducto = @IdProducto;
END;
GO


-- Listar Productos (con Marcas y Categor as)
CREATE OR ALTER PROCEDURE ssp_Producto_Listar
AS
BEGIN
    SELECT P.*,
           M.Descripcion AS NombreMarca,
           C.Descripcion AS NombreCategoria
    FROM PRODUCTO P
    LEFT JOIN MARCA M ON P.IdMarca = M.IdMarca
    LEFT JOIN CATEGORIA C ON P.IdCategoria = C.IdCategoria
    WHERE P.Activo = 1
    ORDER BY P.Nombre;
END;
GO


-- Listar Productos Filtrados por Categor a
CREATE OR ALTER PROCEDURE ssp_Producto_ListarPorCategoria
@IdCategoria INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT P.*, M.Descripcion AS NombreMarca, C.Descripcion AS NombreCategoria
    FROM PRODUCTO P
	LEFT JOIN CATEGORIA C ON P.IdCategoria = C.IdCategoria
    LEFT JOIN MARCA M ON P.IdMarca = M.IdMarca
    WHERE P.Activo = 1 AND P.IdCategoria = @IdCategoria
    ORDER BY P.Nombre ASC;
END;
GO

-- Listar Productos Filtrados por Marca
CREATE OR ALTER PROCEDURE ssp_Producto_ListarPorMarca
@IdMarca INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT P.*, M.Descripcion AS NombreMarca, C.Descripcion AS NombreCategoria
    FROM PRODUCTO P
    LEFT JOIN MARCA M ON P.IdMarca = M.IdMarca
    LEFT JOIN CATEGORIA C ON P.IdCategoria = C.IdCategoria
    WHERE P.Activo = 1 AND P.IdMarca = @IdMarca
    ORDER BY P.Nombre ASC;
END;
GO

-- Listar Productos Agrupados (Vista de inventario)
CREATE OR ALTER PROCEDURE ssp_Producto_ListarAgrupado
AS
BEGIN
    SET NOCOUNT ON;
    SELECT C.Descripcion AS Categoria, P.IdProducto, P.Nombre, P.Precio, P.Stock, M.Descripcion AS Marca
    FROM PRODUCTO P
	LEFT JOIN CATEGORIA C ON P.IdCategoria = C.IdCategoria
    LEFT JOIN MARCA M ON P.IdMarca = M.IdMarca
    WHERE P.Activo = 1
    ORDER BY C.Descripcion, P.Nombre;
END;
GO

-- Obtener Detalle de Producto por ID
CREATE OR ALTER PROCEDURE ssp_Producto_ObtenerDetalle
@IdProducto INT
AS
BEGIN
    SELECT P.*,
           M.Descripcion AS NombreMarca,
           C.Descripcion AS NombreCategoria
    FROM PRODUCTO P
    LEFT JOIN MARCA M ON P.IdMarca = M.IdMarca
    LEFT JOIN CATEGORIA C ON P.IdCategoria = C.IdCategoria
    WHERE P.IdProducto = @IdProducto;
END;
GO


-- Eliminaci n L gica (Activo = 0)
CREATE OR ALTER PROCEDURE ssp_Producto_Eliminar
@IdProducto INT
AS
BEGIN
    UPDATE PRODUCTO SET Activo = 0 WHERE IdProducto = @IdProducto;
END;
GO


-- Eliminaci n F sica (Precauci n: solo para desarrollo)
CREATE OR ALTER PROCEDURE ssp_Producto_EliminarFisico
    @IdProducto INT
AS
BEGIN
    DELETE FROM PRODUCTO WHERE IdProducto = @IdProducto;
END;
GO

--------------------------------------------------------------------------------
-- 8. TRANSACCIONES Y SEGURIDAD (Login, Venta, Clave)
--------------------------------------------------------------------------------

-- Registro de Venta (Usa TVP dbo.DetalleVentaType y Transacci n)
CREATE OR ALTER PROCEDURE ssp_RegistrarVenta
(
    @IdCliente INT,
    @Contacto VARCHAR(50),
    @IdDistrito VARCHAR(6),
    @Telefono VARCHAR(50),
    @Direccion VARCHAR(500),
    @IdTransaccion VARCHAR(50),
    @IdUsuarioEmpleado INT,
    @DetallesVenta dbo.DetalleVentaType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM CLIENTE WHERE IdCliente = @IdCliente AND Activo = 1)
    BEGIN
        RAISERROR('Cliente inv lido', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM @DetallesVenta D
        INNER JOIN PRODUCTO P ON P.IdProducto = D.IdProducto
        WHERE P.Stock < D.Cantidad
    )
    BEGIN
        RAISERROR('Stock insuficiente', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @IdVenta INT;

        INSERT INTO VENTA
        (IdCliente, TotalProducto, MontoTotal, Contacto,
         IdDistrito, Telefono, Direccion, IdTransaccion, IdUsuarioEmpleado)
        VALUES
        (@IdCliente, 0, 0, @Contacto,
         @IdDistrito, @Telefono, @Direccion, @IdTransaccion, @IdUsuarioEmpleado);

        SET @IdVenta = SCOPE_IDENTITY();

        INSERT INTO DETALLE_VENTA (IdVenta, IdProducto, Cantidad, Total)
        SELECT @IdVenta, IdProducto, Cantidad, Total
        FROM @DetallesVenta;

        UPDATE P
        SET P.Stock = P.Stock - D.Cantidad
        FROM PRODUCTO P
        INNER JOIN @DetallesVenta D ON P.IdProducto = D.IdProducto;

        UPDATE VENTA
        SET TotalProducto = (SELECT SUM(Cantidad) FROM @DetallesVenta),
            MontoTotal = (SELECT SUM(Total) FROM @DetallesVenta)
        WHERE IdVenta = @IdVenta;

        COMMIT TRANSACTION;

        SELECT @IdVenta AS IdVenta;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Validar Login de Empleado/Usuario
CREATE OR ALTER PROCEDURE ssp_Usuario_Validar
@Correo VARCHAR(100),
@PasswordHash VARCHAR(512)
AS
BEGIN
    SELECT *
    FROM USUARIO
    WHERE Correo = @Correo AND PasswordHash = @PasswordHash AND Activo = 1;
END;
GO

-- Validar Login de Cliente
CREATE OR ALTER PROCEDURE ssp_Cliente_Validar
@Correo VARCHAR(100),
@PasswordHash VARCHAR(512)
AS
BEGIN
    SELECT *
    FROM CLIENTE
    WHERE Correo = @Correo AND PasswordHash = @PasswordHash AND Activo = 1;
END;
GO

-- Reestablecer Clave de Usuario (tras cambio de contrase a)
CREATE OR ALTER PROCEDURE ssp_Usuario_ReestablecerClave
@IdUsuario INT,
@PasswordHash VARCHAR(512)
AS
BEGIN
    UPDATE USUARIO SET
        PasswordHash = @PasswordHash,
        Reestablecer = 0
    WHERE IdUsuario = @IdUsuario;
END;
GO

----Notificaicones de clientes e usuairos
CREATE OR ALTER PROCEDURE ssp_Notificacion_Insertar
@IdUsuario INT,
@Mensaje VARCHAR(500)
AS
BEGIN
    INSERT INTO NOTIFICACION (IdUsuario, Mensaje, EsLeido)
    VALUES (@IdUsuario, @Mensaje, 0);

    SELECT SCOPE_IDENTITY() AS IdNotificacion;
END;
GO

--
CREATE OR ALTER PROCEDURE ssp_Notificacion_Listar
@IdUsuario INT
AS
BEGIN
    SELECT *
    FROM NOTIFICACION
    WHERE IdUsuario = @IdUsuario
    ORDER BY FechaCreacion DESC;
END;
GO

--
CREATE OR ALTER PROCEDURE ssp_Notificacion_MarcarLeido
@IdNotificacion INT
AS
BEGIN
    UPDATE NOTIFICACION
    SET EsLeido = 1
    WHERE IdNotificacion = @IdNotificacion;
END;
GO

--
CREATE OR ALTER PROCEDURE ssp_Notificacion_Eliminar
@IdNotificacion INT
AS
BEGIN
    DELETE FROM NOTIFICACION
    WHERE IdNotificacion = @IdNotificacion;
END;
GO

