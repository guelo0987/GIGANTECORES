using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIGANTECORE.Migrations
{
    /// <inheritdoc />
    public partial class RoleUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. First create the Roles table
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRol);
                });

            // 2. Add default roles
            migrationBuilder.Sql(@"
                INSERT INTO Roles (Name) VALUES ('Administrador');
                INSERT INTO Roles (Name) VALUES ('Cliente');
            ");

            // 3. Add the new columns
            migrationBuilder.AddColumn<int>(
                name: "RolId",
                table: "UsuarioCliente",
                type: "int",
                nullable: true);  // Temporarily nullable

            migrationBuilder.AddColumn<int>(
                name: "RolId",
                table: "Admin",
                type: "int",
                nullable: true);

            // 4. Migrate existing data
            migrationBuilder.Sql(@"
                UPDATE Admin 
                SET RolId = (SELECT IdRol FROM Roles WHERE Name = 'Administrador')
                WHERE Rol = 'Administrador';

                UPDATE UsuarioCliente 
                SET RolId = (SELECT IdRol FROM Roles WHERE Name = 'Cliente')
                WHERE Rol = 'Cliente';
            ");

            // 5. Create temporary table for RolePermisos
            migrationBuilder.CreateTable(
                name: "RolePermisosTemp",
                columns: table => new
                {
                    IdPermiso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermisosTemp", x => x.IdPermiso);
                    table.ForeignKey(
                        name: "FK_RolePermisosTemp_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                });

            // 6. Copy data from old RolePermisos to temp table
            migrationBuilder.Sql(@"
                INSERT INTO RolePermisosTemp (RoleId, TableName, CanCreate, CanRead, CanUpdate, CanDelete)
                SELECT r.IdRol, rp.TableName, rp.CanCreate, rp.CanRead, rp.CanUpdate, rp.CanDelete
                FROM RolePermisos rp
                CROSS JOIN Roles r
                WHERE r.Name = rp.Role
            ");

            // 7. Drop old RolePermisos table
            migrationBuilder.DropTable(name: "RolePermisos");

            // 8. Rename temp table to final name
            migrationBuilder.RenameTable(
                name: "RolePermisosTemp",
                newName: "RolePermisos");

            // 9. Make RolId required in UsuarioCliente
            migrationBuilder.AlterColumn<int>(
                name: "RolId",
                table: "UsuarioCliente",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 10. Drop old columns
            migrationBuilder.DropColumn(
                name: "Rol",
                table: "UsuarioCliente");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Admin");

            // 11. Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_UsuarioCliente_RolId",
                table: "UsuarioCliente",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermisos_RoleId",
                table: "RolePermisos",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Admin_RolId",
                table: "Admin",
                column: "RolId");

            // 12. Add foreign key constraints
            migrationBuilder.AddForeignKey(
                name: "FK_Admin_Roles_RolId",
                table: "Admin",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "IdRol",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioCliente_Roles_RolId",
                table: "UsuarioCliente",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "IdRol",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admin_Roles_RolId",
                table: "Admin");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioCliente_Roles_RolId",
                table: "UsuarioCliente");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioCliente_RolId",
                table: "UsuarioCliente");

            migrationBuilder.DropIndex(
                name: "IX_RolePermisos_RoleId",
                table: "RolePermisos");

            migrationBuilder.DropIndex(
                name: "IX_Admin_RolId",
                table: "Admin");

            migrationBuilder.DropColumn(
                name: "RolId",
                table: "UsuarioCliente");

            migrationBuilder.DropColumn(
                name: "RolId",
                table: "Admin");

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "UsuarioCliente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Admin",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Administrador");
        }
    }
}
