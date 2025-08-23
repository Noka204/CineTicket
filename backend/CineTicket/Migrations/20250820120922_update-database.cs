using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class updatedatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__ChiTietHoa__MaHD__4CA06362",
                table: "ChiTietHoaDon");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "ChiTietHoaDon",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaHD",
                table: "ChiTietHoaDon",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DonGia",
                table: "ChiTietHoaDon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK__ChiTietHoa__MaHD__4CA06362",
                table: "ChiTietHoaDon",
                column: "MaHD",
                principalTable: "HoaDon",
                principalColumn: "MaHD",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__ChiTietHoa__MaHD__4CA06362",
                table: "ChiTietHoaDon");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "ChiTietHoaDon",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaHD",
                table: "ChiTietHoaDon",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "DonGia",
                table: "ChiTietHoaDon",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK__ChiTietHoa__MaHD__4CA06362",
                table: "ChiTietHoaDon",
                column: "MaHD",
                principalTable: "HoaDon",
                principalColumn: "MaHD");
        }
    }
}
