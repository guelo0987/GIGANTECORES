using System;
using System.Collections.Generic;
using GIGANTECORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GIGANTECORE.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<admin> admin { get; set; }

    public virtual DbSet<banner> banner { get; set; }

    public virtual DbSet<categoria> categoria { get; set; }

    public virtual DbSet<compañia> compañia { get; set; }

    public virtual DbSet<DetalleSolicitud> DetalleSolicituds { get; set; }

    public virtual DbSet<HistorialCorreo> HistorialCorreos { get; set; }

    public virtual DbSet<productos> productos { get; set; }

    public virtual DbSet<roles> roles { get; set; }

    public virtual DbSet<rolepermisos> rolepermisos { get; set; }

    public virtual DbSet<Solicitud> Solicituds { get; set; }

    public virtual DbSet<subcategoria> subcategoria { get; set; }

    public virtual DbSet<usuario_cliente> usuario_cliente { get; set; }
    
    public virtual DbSet<Carrito> Carrito { get; set; }

    public virtual DbSet<vacantes> vacantes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set PostgreSQL conventions - lowercase table names
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName().ToLower());
            
            // Use snake_case naming convention for PostgreSQL columns
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToLower());
            }
        }
        
        modelBuilder.Entity<admin>(entity =>
        {
            entity.ToTable("admin");
            entity.HasKey(e => e.Id).HasName("PK_admin");

            entity.HasIndex(e => e.RolId, "IX_Admin_RolId");

            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Mail).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.SoloLectura).HasDefaultValue(false);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Admins)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carrito__3214EC0784CC7F99");

            entity.ToTable("Carrito");

            entity.HasIndex(e => e.ProductoId, "IX_Carrito_ProductoId");

            entity.HasIndex(e => e.UsuarioId, "IX_Carrito_UsuarioId");

            entity.HasOne(d => d.Productos).WithMany(p => p.Carritos)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carrito__Product__6C190EBB");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Carritos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carrito__Usuario__46E78A0C");
        });

        modelBuilder.Entity<categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07791AF110");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<compañia>(entity =>
        {
            entity.HasKey(e => e.Rnc).HasName("PK__Compañia__CAFF6951C669784F");

            entity.Property(e => e.Rnc)
                .HasMaxLength(11)
                .HasColumnName("RNC");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<DetalleSolicitud>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__DetalleS__E43646A58EBFD1A7");

            entity.ToTable("DetalleSolicitud");

            entity.HasIndex(e => e.IdSolicitud, "IX_DetalleSolicitud_IdSolicitud");

            entity.HasIndex(e => e.ProductoId, "IX_DetalleSolicitud_ProductoId");

            entity.HasOne(d => d.IdSolicitudNavigation).WithMany(p => p.DetalleSolicituds)
                .HasForeignKey(d => d.IdSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetalleSo__IdSol__4D94879B");

            entity.HasOne(d => d.Productos).WithMany(p => p.DetalleSolicituds)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetalleSo__Produ__6D0D32F4");
        });

        modelBuilder.Entity<HistorialCorreo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Historia__3214EC0765966D64");

            entity.ToTable("HistorialCorreo");

            entity.HasIndex(e => e.DetalleSolicitudId, "IX_HistorialCorreo_DetalleSolicitudId");

            entity.HasIndex(e => e.UsuarioId, "IX_HistorialCorreo_UsuarioId");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Enviado");

            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DetalleSolicitud)
                .WithMany(p => p.HistorialCorreos)
                .HasForeignKey(d => d.DetalleSolicitudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HistorialCorreo__DetalleSolicitudId");

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.HistorialCorreos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HistorialCorreo__UsuarioId");
        });

        modelBuilder.Entity<productos>(entity =>
        {
            entity.HasKey(e => e.Codigo).HasName("PK__tmp_ms_x__06370DAD53EE836F");

            entity.HasIndex(e => e.CategoriaId, "IX_Productos_CategoriaId");

            entity.HasIndex(e => e.SubCategoriaId, "IX_Productos_SubCategoriaId");

            entity.Property(e => e.Codigo).ValueGeneratedNever();
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Marca).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Stock).HasDefaultValue(true);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .HasConstraintName("FK_Productos_Categoria");

            entity.HasOne(d => d.SubCategoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.SubCategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productos__SubCa__6EF57B66");
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PK__Solicitu__36899CEF05DB70D6");

            entity.ToTable("Solicitud");

            entity.HasIndex(e => e.UsuarioId, "IX_Solicitud_UsuarioId");

            entity.Property(e => e.FechaSolicitud)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Solicituds)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Solicitud__Usuar__4AB81AF0");
        });

        modelBuilder.Entity<subcategoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SubCateg__3214EC079C3939DB");

            entity.HasIndex(e => e.CategoriaId, "IX_SubCategoria_CategoriaId");

            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.Categoria).WithMany(p => p.SubCategoria)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SubCatego__Categ__3F466844");
        });

        modelBuilder.Entity<usuario_cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UsuarioC__3214EC07D79F99F6");

            entity.ToTable("UsuarioCliente");

            entity.HasIndex(e => e.Rnc, "IX_UsuarioCliente_RNC");

            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.Ciudad).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Rnc)
                .HasMaxLength(11)
                .HasColumnName("RNC");
            entity.Property(e => e.RolId).IsRequired();
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.RncNavigation).WithMany(p => p.UsuarioClientes)
                .HasForeignKey(d => d.Rnc)
                .HasConstraintName("FK_UsuarioCliente_Compañia");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.UsuarioClientes)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<rolepermisos>(entity =>
        {
            entity.HasKey(e => e.IdPermiso);
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CanCreate).IsRequired();
            entity.Property(e => e.CanRead).IsRequired();
            entity.Property(e => e.CanUpdate).IsRequired();
            entity.Property(e => e.CanDelete).IsRequired();

            entity.HasOne(d => d.Role)
                .WithMany(p => p.RolePermisos)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATA_BASE_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión no está configurada");
            }
            
            optionsBuilder.UseNpgsql(connectionString, options => 
                options.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null)
            );
        }
    }
}