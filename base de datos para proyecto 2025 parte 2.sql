
USE master;
GO

-- 1) Si existe, elimina la BD (igual que tu script original)
IF DB_ID('proyectodiciembre2025') IS NOT NULL
BEGIN
    ALTER DATABASE proyectodiciembre2025 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE proyectodiciembre2025;
END
GO

-- 2) Crear base de datos
CREATE DATABASE proyectodiciembre2025
COLLATE Modern_Spanish_CI_AI;
GO

USE proyectodiciembre2025;
GO

SET NOCOUNT ON;
GO
SET DATEFORMAT ymd;
GO

--------------------------------------------------------------------------------
-- TABLAS DE CATALOGO
--------------------------------------------------------------------------------
CREATE TABLE CATEGORIA(
    IdCategoria INT PRIMARY KEY IDENTITY, 
    Descripcion VARCHAR(100),
    Activo BIT DEFAULT 1,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE MARCA(
    IdMarca INT PRIMARY KEY IDENTITY,
    Descripcion VARCHAR(100),
    Activo BIT DEFAULT 1,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- TABLAS DE UBICACION
--------------------------------------------------------------------------------
CREATE TABLE DEPARTAMENTO (
    IdDepartamento VARCHAR(2) PRIMARY KEY,
    Descripcion VARCHAR(45) NOT NULL
);
GO

CREATE TABLE PROVINCIA (
    IdProvincia VARCHAR(4) PRIMARY KEY,
    Descripcion VARCHAR(45) NOT NULL, 
    IdDepartamento VARCHAR(2) NOT NULL REFERENCES DEPARTAMENTO(IdDepartamento)
);
GO 

CREATE TABLE DISTRITO (
    IdDistrito VARCHAR(6) PRIMARY KEY,
    Descripcion VARCHAR(45) NOT NULL, 
    IdProvincia VARCHAR(4) NOT NULL REFERENCES PROVINCIA(IdProvincia),
    IdDepartamento VARCHAR(2) NOT NULL REFERENCES DEPARTAMENTO(IdDepartamento)
);
GO

--------------------------------------------------------------------------------
-- ROLES Y USUARIOS (SEGURIDAD)
-- NOTA: usamos PasswordHash (no almacenamos Clave)
--------------------------------------------------------------------------------
CREATE TABLE ROL (
    IdRol INT PRIMARY KEY IDENTITY,
    Descripcion VARCHAR(50), -- Ej: 'Administrador', 'Empleado', 'Cliente'
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE USUARIO (
    IdUsuario INT PRIMARY KEY IDENTITY,
    IdRol INT NOT NULL REFERENCES ROL(IdRol), -- Admin / Empleado
    Nombres VARCHAR(100),
    Apellidos VARCHAR(100),
    Correo VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(512) NULL, -- almacenar hash seguro (bcrypt/ASP.NET Identity)
    Reestablecer BIT DEFAULT 1,
    Activo BIT DEFAULT 1,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

-- UNIQUE en correo de usuario
ALTER TABLE USUARIO
    ADD CONSTRAINT UQ_USUARIO_Correo UNIQUE (Correo);
GO

--------------------------------------------------------------------------------
-- CLIENTES (compradores)
-- También usan PasswordHash en vez de Clave
--------------------------------------------------------------------------------
CREATE TABLE CLIENTE (
    IdCliente INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(100),
    Apellidos VARCHAR(100),
    Correo VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(512) NULL,
    Reestablecer BIT DEFAULT 0,
	Telefono VARCHAR(50),
	Activo BIT DEFAULT 1,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

-- UNIQUE en correo de cliente
/*ALTER TABLE CLIENTE
    ADD CONSTRAINT UQ_CLIENTE_Correo UNIQUE (Correo);
GO*/

--------------------------------------------------------------------------------
-- PRODUCTOS y DETALLES
--------------------------------------------------------------------------------
CREATE TABLE PRODUCTO (
    IdProducto INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(500),
    Descripcion VARCHAR(500),
    IdMarca INT NULL REFERENCES MARCA(IdMarca),
    IdCategoria INT NULL REFERENCES CATEGORIA(IdCategoria),
    Precio DECIMAL(10,2) DEFAULT 0,
    Stock INT DEFAULT 0,
    RutaImagen VARCHAR(100) NULL,
    NombreImagen VARCHAR(100) NULL,
    Activo BIT DEFAULT 1,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

-- Imágenes múltiples por producto
CREATE TABLE PRODUCTO_IMAGEN (
    IdImagen INT IDENTITY(1,1) PRIMARY KEY,
    IdProducto INT NOT NULL REFERENCES PRODUCTO(IdProducto),
    Url VARCHAR(500) NOT NULL,
    NombreArchivo VARCHAR(200) NULL,
    EsPrincipal BIT DEFAULT 0,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- LISTA DESEOS (artículos guardados por el cliente)
--------------------------------------------------------------------------------
CREATE TABLE LISTA_DESEOS (
    IdListaDeseos INT PRIMARY KEY IDENTITY,
    IdCliente INT NOT NULL REFERENCES CLIENTE(IdCliente),
    IdProducto INT NOT NULL REFERENCES PRODUCTO(IdProducto),
    FechaGuardado DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- VENTAS y DETALLE_VENTA
--------------------------------------------------------------------------------
CREATE TABLE VENTA (
    IdVenta INT PRIMARY KEY IDENTITY,
    IdCliente INT NOT NULL REFERENCES CLIENTE(IdCliente),
    TotalProducto INT DEFAULT 0, -- cantidad total de items (opcional)
    MontoTotal DECIMAL(10,2) DEFAULT 0,
    Contacto VARCHAR(50) NULL,
    IdDistrito VARCHAR(6) NULL REFERENCES DISTRITO(IdDistrito),
    Telefono VARCHAR(50) NULL,
    Direccion VARCHAR(500) NULL,
    IdTransaccion VARCHAR(50) NULL,
    FechaVenta DATETIME DEFAULT GETDATE(),
    -- Quién procesó la venta (empleado) - nullable
    IdUsuarioEmpleado INT NULL REFERENCES USUARIO(IdUsuario)
);
GO

CREATE TABLE DETALLE_VENTA(
    IdDetalleVenta INT PRIMARY KEY IDENTITY,
    IdVenta INT NOT NULL REFERENCES VENTA(IdVenta),
    IdProducto INT NOT NULL REFERENCES PRODUCTO(IdProducto),
    Cantidad INT NOT NULL DEFAULT 1, 
    Total DECIMAL(10,2) NOT NULL
);
GO

--------------------------------------------------------------------------------
-- TICKETS (soporte) y LOG de tickets
--------------------------------------------------------------------------------
CREATE TABLE TICKET (
    IdTicket INT PRIMARY KEY IDENTITY,
    Asunto VARCHAR(200) NOT NULL,
    Descripcion TEXT,
    IdClienteOrigen INT NULL REFERENCES CLIENTE(IdCliente), -- Origen: Cliente
    IdUsuarioOrigen INT NULL REFERENCES USUARIO(IdUsuario), -- Origen: Empleado/Admin
    Estado VARCHAR(20) DEFAULT 'Abierto', -- Abierto, Cerrado, Rechazado
    IdUsuarioGestion INT NULL REFERENCES USUARIO(IdUsuario), -- Quién lo gestiona (Admin)
    RespuestaAdmin TEXT,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaCierre DATETIME NULL
);
GO

CREATE TABLE TICKET_LOG(
    IdTicketLog INT IDENTITY(1,1) PRIMARY KEY,
    IdTicket INT NOT NULL REFERENCES TICKET(IdTicket),
    IdUsuario INT NULL, -- quién hizo la acción
    Accion VARCHAR(100), -- 'Asignado','Respondido','Cerrado','Rechazado'
    Comentario VARCHAR(MAX),
    Fecha DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- DIRECCIONES (para clientes) - reutilizable en ventas
--------------------------------------------------------------------------------
CREATE TABLE DIRECCION(
    IdDireccion INT IDENTITY(1,1) PRIMARY KEY,
    IdCliente INT NOT NULL REFERENCES CLIENTE(IdCliente),
    Line1 VARCHAR(250),
    Line2 VARCHAR(250),
    IdDepartamento VARCHAR(2) NULL,
    IdProvincia VARCHAR(4) NULL,
    IdDistrito VARCHAR(6) NULL,
    CodigoPostal VARCHAR(20) NULL,
    EsPredeterminada BIT DEFAULT 0,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- PAGOS / TRANSACCIONES
--------------------------------------------------------------------------------
CREATE TABLE PAGO(
    IdPago INT IDENTITY(1,1) PRIMARY KEY,
    IdVenta INT NOT NULL REFERENCES VENTA(IdVenta),
    MetodoPago VARCHAR(50) NULL, -- 'Tarjeta','Yape','Efectivo','PayPal',...
    Monto DECIMAL(10,2) NOT NULL,
    Moneda VARCHAR(10) DEFAULT 'PEN',
    Estado VARCHAR(50) DEFAULT 'Pendiente', -- Pendiente, Completado, Fallido, Reembolsado
    ReferenciaPago VARCHAR(200) NULL,
    FechaPago DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- NOTIFICACIONES y ACTIVITY LOG (auditoría)
--------------------------------------------------------------------------------
CREATE TABLE NOTIFICACION(
    IdNotificacion INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL, -- SIEMPRE debe haber un usuario
    Mensaje VARCHAR(500) NOT NULL,
    EsLeido BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Notificacion_Usuario 
        FOREIGN KEY (IdUsuario) REFERENCES USUARIO(IdUsuario)
);
GO

CREATE TABLE ACTIVITY_LOG(
    IdLog INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NULL, -- puede ser NULL para acciones del sistema
    Accion VARCHAR(200),
    Detalles VARCHAR(MAX),
    Fecha DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE NOTIFICACION_CLIENTE(
    IdNotificacion INT IDENTITY(1,1) PRIMARY KEY,
    IdCliente INT NOT NULL REFERENCES CLIENTE(IdCliente),
    Mensaje VARCHAR(500) NOT NULL,
    EsLeido BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- SISTEMA DE CARRITO MODERNO: CARTS + CART_ITEMS (REEMPLAZA CARRITO ANTIGUO)
-- NOTA: si ejecutas este script en una BD que tuviera la tabla CARRITO, abajo incluyo
--       la migración de datos desde CARRITO a CARTS/CART_ITEMS y DROP TABLE CARRITO.
--------------------------------------------------------------------------------
CREATE TABLE CARTS(
    CartId INT IDENTITY(1,1) PRIMARY KEY,
    IdCliente INT NULL REFERENCES CLIENTE(IdCliente), -- NULL = carrito anónimo
    Status VARCHAR(20) DEFAULT 'Active', -- Active, Saved, Ordered
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE CART_ITEMS(
    CartItemId INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT NOT NULL REFERENCES CARTS(CartId) ON DELETE CASCADE,
    IdProducto INT NOT NULL REFERENCES PRODUCTO(IdProducto),
    Cantidad INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(10,2) NOT NULL,
    AddedAt DATETIME DEFAULT GETDATE()
);
GO

--------------------------------------------------------------------------------
-- INDICES RECOMENDADOS
--------------------------------------------------------------------------------
CREATE INDEX IX_PRODUCTO_Nombre ON PRODUCTO(Nombre);
CREATE INDEX IX_VENTA_FechaVenta ON VENTA(FechaVenta);
CREATE INDEX IX_DETALLEVENTA_IdVenta ON DETALLE_VENTA(IdVenta);
CREATE INDEX IX_CARTS_IdCliente ON CARTS(IdCliente);
GO




--------------------------------------------------------------------------------
-- INSERCIONES INICIALES (roles y ejemplo de usuario)
--------------------------------------------------------------------------------
INSERT INTO ROL (Descripcion) VALUES ('Administrador'), ('Empleado'), ('Cliente');
GO

-- Usuario de ejemplo (password hash placeholder; reemplaza por hash real)
INSERT INTO USUARIO (IdRol, Nombres, Apellidos, Correo, PasswordHash, Reestablecer, Activo)
VALUES (1, 'Admin', 'Sistema', 'admin@ejemplo.com', 'PASTE_YOUR_HASH_HERE', 0, 1);
GO

--------------------------------------------------------------------------------
-- MIGRACION desde tabla CARRITO ANTIGUA (si existiera)
-- Si ejecutas este script sobre una BD donde existe la tabla CARRITO,
-- descomenta las secciones de migración. En tu caso original, el script
-- recrea la BD así que no hay datos previos; incluyo las instrucciones de migración
-- por si alguna vez las necesitas.
--------------------------------------------------------------------------------
/*
-- Paso A: si existiera datos en CARRITO -> crear carritos por cliente
INSERT INTO CARTS (IdCliente, Status, CreatedAt)
SELECT DISTINCT IdCliente, 'Active', GETDATE()
FROM CARRITO
WHERE IdCliente IS NOT NULL;
GO

-- Paso B: insertar CART_ITEMS a partir de CARRITO (usando precio actual en PRODUCTO)
;WITH c AS (
   SELECT CartId, IdCliente
   FROM CARTS
)
INSERT INTO CART_ITEMS (CartId, IdProducto, Cantidad, UnitPrice, AddedAt)
SELECT c.CartId, car.IdProducto, car.Cantidad, ISNULL(p.Precio, 0), GETDATE()
FROM CARRITO car
JOIN c ON c.IdCliente = car.IdCliente
LEFT JOIN PRODUCTO p ON p.IdProducto = car.IdProducto;
GO

-- Paso C: eliminar tabla CARRITO (solo después de verificar la migración)
DROP TABLE CARRITO;
GO
*/
--------------------------------------------------------------------------------
-- FIN DEL SCRIPT
--------------------------
--------------------------------PARA EL DAO------------------------------------------------------
SET NOCOUNT OFF;
GO
IF EXISTS (SELECT * FROM sys.types WHERE name = 'DetalleVentaType' AND is_user_defined = 1)
BEGIN
    DROP TYPE DetalleVentaType;
END
GO

CREATE TYPE DetalleVentaType AS TABLE
(
    IdProducto INT,
    Cantidad INT,
    Total DECIMAL(10,2)
);
GO

CREATE TABLE Estado_Pedido (
    IdEstadoPedido INT PRIMARY KEY,
    Descripcion VARCHAR(50) NOT NULL
);

INSERT INTO Estado_Pedido (IdEstadoPedido, Descripcion)
VALUES
(1, 'Registrado'),
(2, 'Pagado'),
(3, 'En preparación'),
(4, 'Enviado'),
(5, 'Entregado'),
(6, 'Cancelado');
SELECT * FROM Estado_Pedido;


ALTER TABLE Venta
ADD IdEstadoPedido INT NOT NULL DEFAULT 1;

ALTER TABLE Venta
ADD CONSTRAINT FK_Venta_EstadoPedido
FOREIGN KEY (IdEstadoPedido)
REFERENCES Estado_Pedido(IdEstadoPedido);

UPDATE V
SET MontoTotal = D.TotalCalculado
FROM VENTA V
JOIN (
    SELECT dv.IdVenta,
           SUM(p.Precio * dv.Cantidad) AS TotalCalculado
    FROM DETALLE_VENTA dv
    JOIN PRODUCTO p ON p.IdProducto = dv.IdProducto
    GROUP BY dv.IdVenta
) D ON V.IdVenta = D.IdVenta;
go
SELECT 'la base de datos proyectodicimebre 2025 creada.' AS Mensaje;
GO
