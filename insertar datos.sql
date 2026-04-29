use proyectodiciembre2025
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT ALL";
EXEC sp_msforeachtable "DELETE FROM ?";
EXEC sp_msforeachtable "
IF OBJECTPROPERTY(object_id('?'), 'TableHasIdentity') = 1
    DBCC CHECKIDENT ('?', RESEED, 0)
";
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL";


DECLARE @RolAdmin INT, @RolEmpleado INT, @RolCliente INT;

INSERT INTO ROL (Descripcion) VALUES ('Administrador');
SET @RolAdmin = SCOPE_IDENTITY();

INSERT INTO ROL (Descripcion) VALUES ('Empleado');
SET @RolEmpleado = SCOPE_IDENTITY();

INSERT INTO ROL (Descripcion) VALUES ('Cliente');
SET @RolCliente = SCOPE_IDENTITY();
--
DECLARE @UserAdmin INT, @UserEmpleado INT;

INSERT INTO USUARIO (IdRol, Nombres, Apellidos, Correo, PasswordHash, Reestablecer, Activo)
VALUES (@RolAdmin, 'Admin', 'Principal', 'admin@test.com', 'HASH123', 0, 1);
SET @UserAdmin = SCOPE_IDENTITY();

INSERT INTO USUARIO (IdRol, Nombres, Apellidos, Correo, PasswordHash, Reestablecer, Activo)
VALUES (@RolEmpleado, 'Juan', 'Empleado', 'empleado@test.com', 'HASH123', 0, 1);
SET @UserEmpleado = SCOPE_IDENTITY();
-----------
DECLARE @Cliente1 INT, @Cliente2 INT;

INSERT INTO CLIENTE (Nombre, Apellidos, Correo, PasswordHash, Telefono, Activo, Reestablecer)
VALUES ('Carlos', 'Ramirez', 'carlos@test.com', 'HASH123', '987654321', 1, 0);
SET @Cliente1 = SCOPE_IDENTITY();

INSERT INTO CLIENTE (Nombre, Apellidos, Correo, PasswordHash, Telefono, Activo, Reestablecer)
VALUES ('Ana', 'Torres', 'ana@test.com', 'HASH123', '912345678', 1, 0);
SET @Cliente2 = SCOPE_IDENTITY();
----------
INSERT INTO DEPARTAMENTO VALUES ('01', 'Lima');
INSERT INTO PROVINCIA VALUES ('0101', 'Lima', '01');
INSERT INTO DISTRITO VALUES ('010101', 'Miraflores', '0101', '01');
---------
DECLARE @CatLaptop INT, @CatAcc INT, @CatMon INT;

INSERT INTO CATEGORIA (Descripcion) VALUES ('Laptops');
SET @CatLaptop = SCOPE_IDENTITY();

INSERT INTO CATEGORIA (Descripcion) VALUES ('Accesorios');
SET @CatAcc = SCOPE_IDENTITY();

INSERT INTO CATEGORIA (Descripcion) VALUES ('Monitores');
SET @CatMon = SCOPE_IDENTITY();
-----------
DECLARE @MarcaMSI INT, @MarcaASUS INT, @MarcaLOG INT;

INSERT INTO MARCA (Descripcion) VALUES ('MSI');
SET @MarcaMSI = SCOPE_IDENTITY();

INSERT INTO MARCA (Descripcion) VALUES ('ASUS');
SET @MarcaASUS = SCOPE_IDENTITY();

INSERT INTO MARCA (Descripcion) VALUES ('Logitech');
SET @MarcaLOG = SCOPE_IDENTITY();
----
DECLARE @Prod1 INT, @Prod2 INT, @Prod3 INT;

INSERT INTO PRODUCTO
(Nombre, Descripcion, IdMarca, IdCategoria, Precio, Stock, RutaImagen, NombreImagen)
VALUES
('Laptop MSI Alpha', 'Laptop gamer Ryzen', @MarcaMSI, @CatLaptop, 4500, 15, 'img/', 'msi.jpg');
SET @Prod1 = SCOPE_IDENTITY();

INSERT INTO PRODUCTO
VALUES
('Monitor ASUS 27"', '144Hz IPS Gaming', @MarcaASUS, @CatMon, 1200, 10, 'img/', 'asus.jpg', 1, GETDATE());
SET @Prod2 = SCOPE_IDENTITY();

INSERT INTO PRODUCTO
VALUES
('Mouse Logitech G203', 'Mouse RGB', @MarcaLOG, @CatAcc, 90, 50, 'img/', 'g203.jpg', 1, GETDATE());
SET @Prod3 = SCOPE_IDENTITY();
------
INSERT INTO PRODUCTO_IMAGEN (IdProducto, Url, NombreArchivo, EsPrincipal)
VALUES
(@Prod1, 'img/msi.jpg', 'msi.jpg', 1),
(@Prod2, 'img/asus.jpg', 'asus.jpg', 1),
(@Prod3, 'img/g203.jpg', 'g203.jpg', 1);

----
DECLARE @Cart INT;

INSERT INTO CARTS (IdCliente, Status)
VALUES (@Cliente1, 'Active');
SET @Cart = SCOPE_IDENTITY();

INSERT INTO CART_ITEMS (CartId, IdProducto, Cantidad, UnitPrice)
VALUES
(@Cart, @Prod1, 1, 4500),
(@Cart, @Prod3, 2, 90);
--
INSERT INTO Estado_Pedido (IdEstadoPedido, Descripcion)
VALUES
(1, 'Registrado'),
(2, 'Pagado'),
(3, 'En preparación'),
(4, 'Enviado'),
(5, 'Entregado'),
(6, 'Cancelado');


DECLARE @VentaCarlos INT;

INSERT INTO VENTA
(
    IdCliente,
    TotalProducto,
    MontoTotal,
    Contacto,
    IdDistrito,
    Telefono,
    Direccion,
    IdTransaccion,
    IdUsuarioEmpleado,
    IdEstadoPedido
)
VALUES
(
    @Cliente1,
    3,
    4680,
    'Carlos Ramirez',
    '010101',
    '987654321',
    'Av Lima 123',
    'TX123',
    @UserEmpleado,
    1 -- Registrado
);

SET @VentaCarlos = SCOPE_IDENTITY();
INSERT INTO DETALLE_VENTA (IdVenta, IdProducto, Cantidad, Total)
VALUES
(@VentaCarlos, @Prod1, 1, 4500),
(@VentaCarlos, @Prod3, 2, 180);

-----
INSERT INTO NOTIFICACION (IdUsuario, Mensaje)
VALUES
(@UserAdmin, 'Nueva venta registrada'),
(@UserEmpleado, 'Venta asignada');
--------
DECLARE @Ticket INT;

INSERT INTO TICKET (Asunto, Descripcion, IdClienteOrigen, Estado)
VALUES ('Demora', 'Pedido no llega', @Cliente1, 'Abierto');
SET @Ticket = SCOPE_IDENTITY();

INSERT INTO TICKET_LOG (IdTicket, Accion, Comentario)
VALUES (@Ticket, 'Creado', 'Cliente abrió ticket');

select * from NOTIFICACION
go
select * from CLIENTE
go
select * from Marca
go
select * from producto
go

select * from usuario 
go
			
select * from rol
go
select * from VENTA
go
select * from CLIENTE
go
select * from detalle_venta
select *from  CART_ITEMS
go
SELECT *
FROM DETALLE_VENTA
WHERE IdVenta = 1;
SELECT * FROM NOTIFICACION_CLIENTE WHERE IdCliente = 1;

SELECT * 
FROM NOTIFICACION_CLIENTE 
WHERE IdCliente = 1;