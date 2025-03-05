using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIGANTECORE.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Administrador"),
                    FechaIngreso = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SoloLectura = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Admin__3214EC07A4E8858C", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__3214EC07791AF110", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Compañia",
                columns: table => new
                {
                    RNC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Compañia__CAFF6951C669784F", x => x.RNC);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SubCateg__3214EC079C3939DB", x => x.Id);
                    table.ForeignKey(
                        name: "FK__SubCatego__Categ__3F466844",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsuarioCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RNC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    DOB = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UsuarioC__3214EC07D79F99F6", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioCliente_Compañia",
                        column: x => x.RNC,
                        principalTable: "Compañia",
                        principalColumn: "RNC");
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Codigo = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Stock = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    SubCategoriaId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tmp_ms_x__06370DAD53EE836F", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_Productos_Categoria",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__Productos__SubCa__6EF57B66",
                        column: x => x.SubCategoriaId,
                        principalTable: "SubCategoria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Solicitud",
                columns: table => new
                {
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Solicitu__36899CEF05DB70D6", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK__Solicitud__Usuar__4AB81AF0",
                        column: x => x.UsuarioId,
                        principalTable: "UsuarioCliente",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Carrito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Carrito__3214EC0784CC7F99", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Carrito__Product__6C190EBB",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Codigo");
                    table.ForeignKey(
                        name: "FK__Carrito__Usuario__46E78A0C",
                        column: x => x.UsuarioId,
                        principalTable: "UsuarioCliente",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetalleSolicitud",
                columns: table => new
                {
                    IdDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSolicitud = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DetalleS__E43646A58EBFD1A7", x => x.IdDetalle);
                    table.ForeignKey(
                        name: "FK__DetalleSo__IdSol__4D94879B",
                        column: x => x.IdSolicitud,
                        principalTable: "Solicitud",
                        principalColumn: "IdSolicitud");
                    table.ForeignKey(
                        name: "FK__DetalleSo__Produ__6D0D32F4",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Codigo");
                });

            migrationBuilder.CreateTable(
                name: "HistorialCorreo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Enviado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Historia__3214EC0765966D64", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Historial__Solic__66603565",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitud",
                        principalColumn: "IdSolicitud");
                    table.ForeignKey(
                        name: "FK__Historial__Usuar__656C112C",
                        column: x => x.UsuarioId,
                        principalTable: "UsuarioCliente",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carrito_ProductoId",
                table: "Carrito",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Carrito_UsuarioId",
                table: "Carrito",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSolicitud_IdSolicitud",
                table: "DetalleSolicitud",
                column: "IdSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSolicitud_ProductoId",
                table: "DetalleSolicitud",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCorreo_SolicitudId",
                table: "HistorialCorreo",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCorreo_UsuarioId",
                table: "HistorialCorreo",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_SubCategoriaId",
                table: "Productos",
                column: "SubCategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_UsuarioId",
                table: "Solicitud",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoria_CategoriaId",
                table: "SubCategoria",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioCliente_RNC",
                table: "UsuarioCliente",
                column: "RNC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "Carrito");

            migrationBuilder.DropTable(
                name: "DetalleSolicitud");

            migrationBuilder.DropTable(
                name: "HistorialCorreo");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Solicitud");

            migrationBuilder.DropTable(
                name: "SubCategoria");

            migrationBuilder.DropTable(
                name: "UsuarioCliente");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Compañia");
        }
    }
}
