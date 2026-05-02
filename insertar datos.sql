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

-- =================================================================
-- CLIENTES EXTRA (para probar paginacion)
-- Password de todos: HASH123
-- =================================================================
INSERT INTO CLIENTE (Nombre, Apellidos, Correo, PasswordHash, Telefono, Activo, Reestablecer)
VALUES
('Maria',     'Gonzales',   'maria@test.com',     'HASH123', '987111222', 1, 0),
('Jose',      'Mendoza',    'jose@test.com',      'HASH123', '987222333', 1, 0),
('Lucia',     'Vargas',     'lucia@test.com',     'HASH123', '987333444', 1, 0),
('Pedro',     'Quispe',     'pedro@test.com',     'HASH123', '987444555', 1, 0),
('Rosa',      'Chavez',     'rosa@test.com',      'HASH123', '987555666', 1, 0),
('Diego',     'Salazar',    'diego@test.com',     'HASH123', '987666777', 1, 0),
('Valeria',   'Castillo',   'valeria@test.com',   'HASH123', '987777888', 1, 0),
('Andres',    'Flores',     'andres@test.com',    'HASH123', '987888999', 1, 0),
('Camila',    'Rojas',      'camila@test.com',    'HASH123', '987999000', 1, 0),
('Miguel',    'Paredes',    'miguel@test.com',    'HASH123', '988111222', 1, 0),
('Daniela',   'Soto',       'daniela@test.com',   'HASH123', '988222333', 1, 0),
('Fernando',  'Rios',       'fernando@test.com',  'HASH123', '988333444', 1, 0),
('Alejandra', 'Medina',     'alejandra@test.com', 'HASH123', '988444555', 1, 0),
('Ricardo',   'Ortiz',      'ricardo@test.com',   'HASH123', '988555666', 1, 0),
('Patricia',  'Aguilar',    'patricia@test.com',  'HASH123', '988666777', 1, 0);
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

-- =================================================================
-- IMAGENES: usamos picsum.photos con seed por producto.
-- Cada seed devuelve siempre la misma imagen (consistente, alta calidad).
-- Si quieres una imagen especifica, sube una desde el panel admin
-- (Editar producto -> uploader) y reemplaza la URL.
-- =================================================================

INSERT INTO PRODUCTO
(Nombre, Descripcion, IdMarca, IdCategoria, Precio, Stock, RutaImagen, NombreImagen, Activo, FechaRegistro)
VALUES
('Laptop MSI Alpha 15', 'Laptop gamer Ryzen 7 RTX 3060 16GB',
 @MarcaMSI, @CatLaptop, 4500, 15,
 'https://picsum.photos/seed/msi-alpha-15/600/400', 'msi-alpha-15.jpg', 1, GETDATE());
SET @Prod1 = SCOPE_IDENTITY();

INSERT INTO PRODUCTO
(Nombre, Descripcion, IdMarca, IdCategoria, Precio, Stock, RutaImagen, NombreImagen, Activo, FechaRegistro)
VALUES
('Monitor ASUS VG279', 'Monitor 27" IPS 144Hz 1ms Gaming',
 @MarcaASUS, @CatMon, 1200, 10,
 'https://picsum.photos/seed/asus-vg279/600/400', 'asus-vg279.jpg', 1, GETDATE());
SET @Prod2 = SCOPE_IDENTITY();

INSERT INTO PRODUCTO
(Nombre, Descripcion, IdMarca, IdCategoria, Precio, Stock, RutaImagen, NombreImagen, Activo, FechaRegistro)
VALUES
('Mouse Logitech G203 Lightsync', 'Mouse gamer 8000 DPI RGB',
 @MarcaLOG, @CatAcc, 90, 50,
 'https://picsum.photos/seed/logitech-g203/600/400', 'logitech-g203.jpg', 1, GETDATE());
SET @Prod3 = SCOPE_IDENTITY();

-- =================================================================
-- PRODUCTOS EXTRA (catalogo amplio para probar paginacion)
-- Total con los 3 anteriores: ~36 productos
-- =================================================================
INSERT INTO PRODUCTO
(Nombre, Descripcion, IdMarca, IdCategoria, Precio, Stock, RutaImagen, NombreImagen, Activo, FechaRegistro)
VALUES
-- LAPTOPS
('Laptop ASUS TUF F15',         'Gaming i7-12700H RTX 4060 16GB 512SSD', @MarcaASUS, @CatLaptop, 5200, 12, 'https://picsum.photos/seed/asus-tuf-f15/600/400',     'asus-tuf-f15.jpg',     1, GETDATE()),
('Laptop MSI Katana 15',        'Gaming i5-13420H RTX 4050 8GB 512SSD',  @MarcaMSI,  @CatLaptop, 4800,  8, 'https://picsum.photos/seed/msi-katana-15/600/400',    'msi-katana-15.jpg',    1, GETDATE()),
('Laptop ASUS Vivobook 15',     'Ultraligera Ryzen 5 8GB 256SSD',        @MarcaASUS, @CatLaptop, 2900, 20, 'https://picsum.photos/seed/asus-vivobook-15/600/400', 'asus-vivobook-15.jpg', 1, GETDATE()),
('Laptop MSI Modern 14',        'Ofimatica Intel i5 8GB 512SSD',         @MarcaMSI,  @CatLaptop, 2500, 18, 'https://picsum.photos/seed/msi-modern-14/600/400',    'msi-modern-14.jpg',    1, GETDATE()),
('Laptop ASUS ROG Strix G16',   'Gaming i9 RTX 4070 32GB 1TB SSD',       @MarcaASUS, @CatLaptop, 7800,  5, 'https://picsum.photos/seed/asus-rog-strix-g16/600/400','asus-rog-strix-g16.jpg',1, GETDATE()),
('Laptop MSI Cyborg 15',        'Gaming RTX 4060 16GB pantalla 144Hz',   @MarcaMSI,  @CatLaptop, 4200, 11, 'https://picsum.photos/seed/msi-cyborg-15/600/400',    'msi-cyborg-15.jpg',    1, GETDATE()),
('Laptop ASUS Zenbook 14 OLED', 'Ultradelgada OLED Touch 14"',           @MarcaASUS, @CatLaptop, 5500,  7, 'https://picsum.photos/seed/asus-zenbook-14/600/400',  'asus-zenbook-14.jpg',  1, GETDATE()),
('Laptop MSI Stealth 16 AI',    'Studio RTX 4070 Core Ultra 7',          @MarcaMSI,  @CatLaptop, 8900,  3, 'https://picsum.photos/seed/msi-stealth-16/600/400',   'msi-stealth-16.jpg',   1, GETDATE()),

-- MONITORES
('Monitor MSI Optix G24C',      'Curvo 144Hz 24" Full HD',               @MarcaMSI,  @CatMon, 1100, 14, 'https://picsum.photos/seed/msi-optix-g24c/600/400',   'msi-optix-g24c.jpg',   1, GETDATE()),
('Monitor ASUS TUF VG27',       'IPS 27" 165Hz 1ms G-Sync',              @MarcaASUS, @CatMon, 1450,  9, 'https://picsum.photos/seed/asus-tuf-vg27/600/400',    'asus-tuf-vg27.jpg',    1, GETDATE()),
('Monitor MSI Pro MP243',       'IPS Full HD oficina 24"',               @MarcaMSI,  @CatMon,  650, 22, 'https://picsum.photos/seed/msi-pro-mp243/600/400',    'msi-pro-mp243.jpg',    1, GETDATE()),
('Monitor ASUS ProArt PA328',   '4K UHD para disenadores 32"',           @MarcaASUS, @CatMon, 3200,  4, 'https://picsum.photos/seed/asus-proart-pa328/600/400','asus-proart-pa328.jpg',1, GETDATE()),
('Monitor MSI MAG 274QRF QD',   'Quantum Dot 1440p 165Hz 27"',           @MarcaMSI,  @CatMon, 2100,  6, 'https://picsum.photos/seed/msi-mag-274qrf/600/400',   'msi-mag-274qrf.jpg',   1, GETDATE()),
('Monitor ASUS ROG Swift PG32', '4K 144Hz Gaming Premium 32"',           @MarcaASUS, @CatMon, 4900,  3, 'https://picsum.photos/seed/asus-rog-pg32/600/400',    'asus-rog-pg32.jpg',    1, GETDATE()),

-- ACCESORIOS
('Teclado Logitech G PRO X',    'Mecanico Switches GX Blue intercambiables', @MarcaLOG,  @CatAcc, 520, 30, 'https://picsum.photos/seed/logitech-gprox/600/400',   'logitech-gprox.jpg',   1, GETDATE()),
('Mouse Logitech G502 Hero',    '25K DPI 11 botones programables RGB',       @MarcaLOG,  @CatAcc, 280, 40, 'https://picsum.photos/seed/logitech-g502/600/400',    'logitech-g502.jpg',    1, GETDATE()),
('Audifonos Logitech G733',     'Inalambricos LightSpeed RGB',                @MarcaLOG,  @CatAcc, 650, 18, 'https://picsum.photos/seed/logitech-g733/600/400',    'logitech-g733.jpg',    1, GETDATE()),
('Webcam Logitech C922 Pro',    '1080p 30fps streaming microfono dual',       @MarcaLOG,  @CatAcc, 420, 25, 'https://picsum.photos/seed/logitech-c922/600/400',    'logitech-c922.jpg',    1, GETDATE()),
('Mousepad Logitech G840 XL',   'Tela XL 900x400mm para gaming',              @MarcaLOG,  @CatAcc, 180, 35, 'https://picsum.photos/seed/logitech-g840/600/400',    'logitech-g840.jpg',    1, GETDATE()),
('Teclado MSI Vigor GK50',      'Mecanico Kailh Box White RGB',               @MarcaMSI,  @CatAcc, 380, 28, 'https://picsum.photos/seed/msi-vigor-gk50/600/400',   'msi-vigor-gk50.jpg',   1, GETDATE()),
('Mouse ASUS ROG Gladius III',  'Optico 26K DPI Push-Fit Switch Sockets',     @MarcaASUS, @CatAcc, 350, 24, 'https://picsum.photos/seed/asus-gladius-iii/600/400', 'asus-gladius-iii.jpg', 1, GETDATE()),
('Audifonos ASUS ROG Delta',    '7.1 Surround USB-C Quad-DAC',                @MarcaASUS, @CatAcc, 780, 12, 'https://picsum.photos/seed/asus-rog-delta/600/400',   'asus-rog-delta.jpg',   1, GETDATE()),
('Mouse MSI Clutch GM41',       'Ligero 65g sensor PixArt PAW3389',           @MarcaMSI,  @CatAcc, 220, 40, 'https://picsum.photos/seed/msi-clutch-gm41/600/400',  'msi-clutch-gm41.jpg',  1, GETDATE()),
('Webcam ASUS ROG Eye S',       '1080p 60fps con beamforming microphones',    @MarcaASUS, @CatAcc, 690, 14, 'https://picsum.photos/seed/asus-rog-eye/600/400',     'asus-rog-eye.jpg',     1, GETDATE()),
('Audifonos MSI Immerse GH50',  'Sonido virtual 7.1 RGB Gaming',              @MarcaMSI,  @CatAcc, 410, 20, 'https://picsum.photos/seed/msi-immerse-gh50/600/400', 'msi-immerse-gh50.jpg', 1, GETDATE()),
('Teclado ASUS ROG Strix Scope','Mecanico Cherry MX Red RGB',                 @MarcaASUS, @CatAcc, 720, 16, 'https://picsum.photos/seed/asus-rog-scope/600/400',   'asus-rog-scope.jpg',   1, GETDATE()),
('Mouse Logitech MX Master 3S', 'Productividad inalambrico silencioso',       @MarcaLOG,  @CatAcc, 590, 22, 'https://picsum.photos/seed/logitech-mx-master-3s/600/400','logitech-mx3s.jpg', 1, GETDATE()),
('Teclado Logitech MX Keys',    'Inalambrico minimalista retroiluminado',     @MarcaLOG,  @CatAcc, 680, 19, 'https://picsum.photos/seed/logitech-mx-keys/600/400',  'logitech-mx-keys.jpg', 1, GETDATE());

------
INSERT INTO PRODUCTO_IMAGEN (IdProducto, Url, NombreArchivo, EsPrincipal)
VALUES
(@Prod1, 'https://picsum.photos/seed/msi-alpha-15/600/400',   'msi-alpha-15.jpg',   1),
(@Prod2, 'https://picsum.photos/seed/asus-vg279/600/400',     'asus-vg279.jpg',     1),
(@Prod3, 'https://picsum.photos/seed/logitech-g203/600/400',  'logitech-g203.jpg',  1);

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
(3, 'En preparacion'),
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
VALUES (@Ticket, 'Creado', 'Cliente abrio ticket');

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