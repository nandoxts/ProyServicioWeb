using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyApiProyectoOnline2025.Models;

public partial class Proyectodiciembre2025Context : DbContext
{
    public Proyectodiciembre2025Context()
    {
    }

    public Proyectodiciembre2025Context(DbContextOptions<Proyectodiciembre2025Context> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<DetalleVentum> DetalleVenta { get; set; }

    public virtual DbSet<Direccion> Direccions { get; set; }

    public virtual DbSet<Distrito> Distritos { get; set; }

    public virtual DbSet<ListaDeseo> ListaDeseos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<NotificacionCliente> NotificacionClientes { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductoImagen> ProductoImagens { get; set; }

    public virtual DbSet<Provincium> Provincia { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketLog> TicketLogs { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        //=> optionsBuilder.UseSqlServer("server=DESKTOP-P4D2NDS\\SQLEXPRESS;database=proyectodiciembre2025;integrated security=true;TrustServerCertificate=false;Encrypt=false;");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Modern_Spanish_CI_AI");

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("PK__ACTIVITY__0C54DBC65A4B3BF8");

            entity.ToTable("ACTIVITY_LOG");

            entity.Property(e => e.Accion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Detalles).IsUnicode(false);
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__CARTS__51BCD7B7E5630EAC");

            entity.ToTable("CARTS");

            entity.HasIndex(e => e.IdCliente, "IX_CARTS_IdCliente");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('Active')");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Carts)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK__CARTS__IdCliente__114A936A");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CART_ITE__488B0B0A93FAD7D1");

            entity.ToTable("CART_ITEMS");

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Cantidad).HasDefaultValueSql("((1))");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CART_ITEM__CartI__160F4887");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CART_ITEM__IdPro__17036CC0");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__CATEGORI__A3C02A108EBAF013");

            entity.ToTable("CATEGORIA");

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__CLIENTE__D594664218384FD0");

            entity.ToTable("CLIENTE");

            entity.HasIndex(e => e.Correo, "UQ__CLIENTE__60695A19B935E3C2").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(512)
                .IsUnicode(false);
            entity.Property(e => e.Reestablecer).HasDefaultValueSql("((0))");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PK__DEPARTAM__787A433D384D40F5");

            entity.ToTable("DEPARTAMENTO");

            entity.Property(e => e.IdDepartamento)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(45)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DetalleVentum>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("PK__DETALLE___AAA5CEC2C0BD8F46");

            entity.ToTable("DETALLE_VENTA");

            entity.HasIndex(e => e.IdVenta, "IX_DETALLEVENTA_IdVenta");

            entity.Property(e => e.Cantidad).HasDefaultValueSql("((1))");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DETALLE_V__IdPro__70DDC3D8");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DETALLE_V__IdVen__6FE99F9F");
        });

        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK__DIRECCIO__1F8E0C764A6E7EA0");

            entity.ToTable("DIRECCION");

            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.EsPredeterminada).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdDepartamento)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.IdDistrito)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.IdProvincia)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Line1)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Line2)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Direccions)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DIRECCION__IdCli__7F2BE32F");
        });

        modelBuilder.Entity<Distrito>(entity =>
        {
            entity.HasKey(e => e.IdDistrito).HasName("PK__DISTRITO__DE8EED594CEFA5E3");

            entity.ToTable("DISTRITO");

            entity.Property(e => e.IdDistrito)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.IdDepartamento)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.IdProvincia)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Distritos)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DISTRITO__IdDepa__44FF419A");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Distritos)
                .HasForeignKey(d => d.IdProvincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DISTRITO__IdProv__440B1D61");
        });

        modelBuilder.Entity<ListaDeseo>(entity =>
        {
            entity.HasKey(e => e.IdListaDeseos).HasName("PK__LISTA_DE__1A2466EEFA23DBF1");

            entity.ToTable("LISTA_DESEOS");

            entity.Property(e => e.FechaGuardado)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ListaDeseos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LISTA_DES__IdCli__6383C8BA");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ListaDeseos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LISTA_DES__IdPro__6477ECF3");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PK__MARCA__4076A887D0006927");

            entity.ToTable("MARCA");

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__NOTIFICA__F6CA0A85B961A161");

            entity.ToTable("NOTIFICACION");

            entity.Property(e => e.EsLeido).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Usuario");
        });

        modelBuilder.Entity<NotificacionCliente>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__NOTIFICA__F6CA0A85F274D922");

            entity.ToTable("NOTIFICACION_CLIENTE");

            entity.Property(e => e.EsLeido).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.NotificacionClientes)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NOTIFICAC__IdCli__4F47C5E3");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__PAGO__FC851A3A772E7E28");

            entity.ToTable("PAGO");

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("('Pendiente')");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Moneda)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("('PEN')");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ReferenciaPago)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAGO__IdVenta__03F0984C");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__PRODUCTO__09889210CDCC79FF");

            entity.ToTable("PRODUCTO");

            entity.HasIndex(e => e.Nombre, "IX_PRODUCTO_Nombre");

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.NombreImagen)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Precio)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RutaImagen)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Stock).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK__PRODUCTO__IdCate__5812160E");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMarca)
                .HasConstraintName("FK__PRODUCTO__IdMarc__571DF1D5");
        });

        modelBuilder.Entity<ProductoImagen>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__PRODUCTO__B42D8F2A04112ECD");

            entity.ToTable("PRODUCTO_IMAGEN");

            entity.Property(e => e.EsPrincipal).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Url)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ProductoImagens)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTO___IdPro__5EBF139D");
        });

        modelBuilder.Entity<Provincium>(entity =>
        {
            entity.HasKey(e => e.IdProvincia).HasName("PK__PROVINCI__EED74455BEC21CB6");

            entity.ToTable("PROVINCIA");

            entity.Property(e => e.IdProvincia)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.IdDepartamento)
                .HasMaxLength(2)
                .IsUnicode(false);

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Provincia)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROVINCIA__IdDep__412EB0B6");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__ROL__2A49584C8EF8C475");

            entity.ToTable("ROL");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PK__TICKET__4B93C7E764C6AEA3");

            entity.ToTable("TICKET");

            entity.Property(e => e.Asunto)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('Abierto')");
            entity.Property(e => e.FechaCierre).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RespuestaAdmin).HasColumnType("text");

            entity.HasOne(d => d.IdClienteOrigenNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdClienteOrigen)
                .HasConstraintName("FK__TICKET__IdClient__74AE54BC");

            entity.HasOne(d => d.IdUsuarioGestionNavigation).WithMany(p => p.TicketIdUsuarioGestionNavigations)
                .HasForeignKey(d => d.IdUsuarioGestion)
                .HasConstraintName("FK__TICKET__IdUsuari__778AC167");

            entity.HasOne(d => d.IdUsuarioOrigenNavigation).WithMany(p => p.TicketIdUsuarioOrigenNavigations)
                .HasForeignKey(d => d.IdUsuarioOrigen)
                .HasConstraintName("FK__TICKET__IdUsuari__75A278F5");
        });

        modelBuilder.Entity<TicketLog>(entity =>
        {
            entity.HasKey(e => e.IdTicketLog).HasName("PK__TICKET_L__171B5A05FC830D5A");

            entity.ToTable("TICKET_LOG");

            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Comentario).IsUnicode(false);
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.TicketLogs)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TICKET_LO__IdTic__7B5B524B");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__USUARIO__5B65BF97354AF7FD");

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Correo, "UQ_USUARIO_Correo").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(512)
                .IsUnicode(false);
            entity.Property(e => e.Reestablecer).HasDefaultValueSql("((1))");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USUARIO__IdRol__4AB81AF0");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PK__VENTA__BC1240BDB60CADD1");

            entity.ToTable("VENTA");

            entity.HasIndex(e => e.FechaVenta, "IX_VENTA_FechaVenta");

            entity.Property(e => e.Contacto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaVenta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdDistrito)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.IdTransaccion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MontoTotal)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalProducto).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VENTA__IdCliente__68487DD7");

            entity.HasOne(d => d.IdDistritoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdDistrito)
                .HasConstraintName("FK__VENTA__IdDistrit__6B24EA82");

            entity.HasOne(d => d.IdUsuarioEmpleadoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuarioEmpleado)
                .HasConstraintName("FK__VENTA__IdUsuario__6D0D32F4");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
