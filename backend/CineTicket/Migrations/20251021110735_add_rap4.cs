using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class add_rap4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaRap",
                table: "SuatChieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RapMaRap",
                table: "SuatChieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenRap",
                table: "SuatChieu",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SuatChieu_RapMaRap",
                table: "SuatChieu",
                column: "RapMaRap");

            migrationBuilder.AddForeignKey(
                name: "FK_SuatChieu_Rap_RapMaRap",
                table: "SuatChieu",
                column: "RapMaRap",
                principalSchema: "dbo",
                principalTable: "Rap",
                principalColumn: "MaRap");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuatChieu_Rap_RapMaRap",
                table: "SuatChieu");

            migrationBuilder.DropIndex(
                name: "IX_SuatChieu_RapMaRap",
                table: "SuatChieu");

            migrationBuilder.DropColumn(
                name: "MaRap",
                table: "SuatChieu");

            migrationBuilder.DropColumn(
                name: "RapMaRap",
                table: "SuatChieu");

            migrationBuilder.DropColumn(
                name: "TenRap",
                table: "SuatChieu");
        }
    }
}
