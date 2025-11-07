using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class add_rap3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhongChieu_Rap_MaBnNavigationMaRap",
                table: "PhongChieu");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "Rap",
                newName: "Rap",
                newSchema: "dbo");

            migrationBuilder.RenameColumn(
                name: "MaBnNavigationMaRap",
                table: "PhongChieu",
                newName: "RapMaRap");

            migrationBuilder.RenameIndex(
                name: "IX_PhongChieu_MaBnNavigationMaRap",
                table: "PhongChieu",
                newName: "IX_PhongChieu_RapMaRap");

            migrationBuilder.AlterColumn<int>(
                name: "MaRap",
                table: "PhongChieu",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaRap",
                table: "ChiTietHoaDon",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaRapNavigationMaRap",
                table: "ChiTietHoaDon",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaRapNavigationMaRap",
                table: "ChiTietHoaDon",
                column: "MaRapNavigationMaRap");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietHoaDon_Rap_MaRapNavigationMaRap",
                table: "ChiTietHoaDon",
                column: "MaRapNavigationMaRap",
                principalSchema: "dbo",
                principalTable: "Rap",
                principalColumn: "MaRap");

            migrationBuilder.AddForeignKey(
                name: "FK_PhongChieu_Rap_RapMaRap",
                table: "PhongChieu",
                column: "RapMaRap",
                principalSchema: "dbo",
                principalTable: "Rap",
                principalColumn: "MaRap");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietHoaDon_Rap_MaRapNavigationMaRap",
                table: "ChiTietHoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_PhongChieu_Rap_RapMaRap",
                table: "PhongChieu");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietHoaDon_MaRapNavigationMaRap",
                table: "ChiTietHoaDon");

            migrationBuilder.DropColumn(
                name: "MaRap",
                table: "ChiTietHoaDon");

            migrationBuilder.DropColumn(
                name: "MaRapNavigationMaRap",
                table: "ChiTietHoaDon");

            migrationBuilder.RenameTable(
                name: "Rap",
                schema: "dbo",
                newName: "Rap");

            migrationBuilder.RenameColumn(
                name: "RapMaRap",
                table: "PhongChieu",
                newName: "MaBnNavigationMaRap");

            migrationBuilder.RenameIndex(
                name: "IX_PhongChieu_RapMaRap",
                table: "PhongChieu",
                newName: "IX_PhongChieu_MaBnNavigationMaRap");

            migrationBuilder.AlterColumn<int>(
                name: "MaRap",
                table: "PhongChieu",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PhongChieu_Rap_MaBnNavigationMaRap",
                table: "PhongChieu",
                column: "MaBnNavigationMaRap",
                principalTable: "Rap",
                principalColumn: "MaRap");
        }
    }
}
