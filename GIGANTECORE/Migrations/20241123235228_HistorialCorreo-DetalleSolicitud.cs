using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIGANTECORE.Migrations
{
    /// <inheritdoc />
    public partial class HistorialCorreoDetalleSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Historial__Solic__66603565",
                table: "HistorialCorreo");

            migrationBuilder.DropForeignKey(
                name: "FK__Historial__Usuar__656C112C",
                table: "HistorialCorreo");

            migrationBuilder.RenameColumn(
                name: "SolicitudId",
                table: "HistorialCorreo",
                newName: "DetalleSolicitudId");

            migrationBuilder.RenameIndex(
                name: "IX_HistorialCorreo_SolicitudId",
                table: "HistorialCorreo",
                newName: "IX_HistorialCorreo_DetalleSolicitudId");

            migrationBuilder.AddForeignKey(
                name: "FK__HistorialCorreo__DetalleSolicitudId",
                table: "HistorialCorreo",
                column: "DetalleSolicitudId",
                principalTable: "DetalleSolicitud",
                principalColumn: "IdDetalle");

            migrationBuilder.AddForeignKey(
                name: "FK__HistorialCorreo__UsuarioId",
                table: "HistorialCorreo",
                column: "UsuarioId",
                principalTable: "UsuarioCliente",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__HistorialCorreo__DetalleSolicitudId",
                table: "HistorialCorreo");

            migrationBuilder.DropForeignKey(
                name: "FK__HistorialCorreo__UsuarioId",
                table: "HistorialCorreo");

            migrationBuilder.RenameColumn(
                name: "DetalleSolicitudId",
                table: "HistorialCorreo",
                newName: "SolicitudId");

            migrationBuilder.RenameIndex(
                name: "IX_HistorialCorreo_DetalleSolicitudId",
                table: "HistorialCorreo",
                newName: "IX_HistorialCorreo_SolicitudId");

            migrationBuilder.AddForeignKey(
                name: "FK__Historial__Solic__66603565",
                table: "HistorialCorreo",
                column: "SolicitudId",
                principalTable: "Solicitud",
                principalColumn: "IdSolicitud");

            migrationBuilder.AddForeignKey(
                name: "FK__Historial__Usuar__656C112C",
                table: "HistorialCorreo",
                column: "UsuarioId",
                principalTable: "UsuarioCliente",
                principalColumn: "Id");
        }
    }
}
