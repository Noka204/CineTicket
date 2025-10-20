using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class update_rap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaBnNavigationMaRap",
                table: "PhongChieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaRap",
                table: "PhongChieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HinhAnhUrl",
                table: "BapNuoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Rap",
                columns: table => new
                {
                    MaRap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenRap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanhPho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoatDong = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rap", x => x.MaRap);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhongChieu_MaBnNavigationMaRap",
                table: "PhongChieu",
                column: "MaBnNavigationMaRap");

            migrationBuilder.AddForeignKey(
                name: "FK_PhongChieu_Rap_MaBnNavigationMaRap",
                table: "PhongChieu",
                column: "MaBnNavigationMaRap",
                principalTable: "Rap",
                principalColumn: "MaRap");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhongChieu_Rap_MaBnNavigationMaRap",
                table: "PhongChieu");

            migrationBuilder.DropTable(
                name: "Rap");

            migrationBuilder.DropIndex(
                name: "IX_PhongChieu_MaBnNavigationMaRap",
                table: "PhongChieu");

            migrationBuilder.DropColumn(
                name: "MaBnNavigationMaRap",
                table: "PhongChieu");

            migrationBuilder.DropColumn(
                name: "MaRap",
                table: "PhongChieu");

            migrationBuilder.DropColumn(
                name: "HinhAnhUrl",
                table: "BapNuoc");
        }
    }
}
