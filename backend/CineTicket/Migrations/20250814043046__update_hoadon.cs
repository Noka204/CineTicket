using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class _update_hoadon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientToken",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaSuat",
                table: "HoaDon",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DonGia",
                table: "ChiTietHoaDon",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientToken",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "MaSuat",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "DonGia",
                table: "ChiTietHoaDon");
        }
    }
}
