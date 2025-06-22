using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class NewHoaDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "T",
                table: "Ghe",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VeNavigationMaVe",
                table: "Ghe",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ghe_VeNavigationMaVe",
                table: "Ghe",
                column: "VeNavigationMaVe");

            migrationBuilder.AddForeignKey(
                name: "FK_Ghe_Ve_VeNavigationMaVe",
                table: "Ghe",
                column: "VeNavigationMaVe",
                principalTable: "Ve",
                principalColumn: "MaVe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ghe_Ve_VeNavigationMaVe",
                table: "Ghe");

            migrationBuilder.DropIndex(
                name: "IX_Ghe_VeNavigationMaVe",
                table: "Ghe");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "T",
                table: "Ghe");

            migrationBuilder.DropColumn(
                name: "VeNavigationMaVe",
                table: "Ghe");
        }
    }
}
