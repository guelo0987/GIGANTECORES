using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GIGANTECORE.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTitleadnDescriptionBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Banners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Banners",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Banners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
