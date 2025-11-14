using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class khuyemai_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonKhuyenMai_HoaDon_HoaDonMaHd",
                table: "HoaDonKhuyenMai");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonKhuyenMai_KhuyenMai_KhuyenMaiId",
                table: "HoaDonKhuyenMai");

            migrationBuilder.DropForeignKey(
                name: "FK_KhuyenMaiCode_KhuyenMai_KhuyenMaiId",
                table: "KhuyenMaiCode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KhuyenMaiCode",
                table: "KhuyenMaiCode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KhuyenMai",
                table: "KhuyenMai");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HoaDonKhuyenMai",
                table: "HoaDonKhuyenMai");

            migrationBuilder.RenameTable(
                name: "KhuyenMaiCode",
                newName: "KhuyenMaiCodes");

            migrationBuilder.RenameTable(
                name: "KhuyenMai",
                newName: "KhuyenMais");

            migrationBuilder.RenameTable(
                name: "HoaDonKhuyenMai",
                newName: "HoaDonKhuyenMais");

            migrationBuilder.RenameIndex(
                name: "IX_KhuyenMaiCode_KhuyenMaiId",
                table: "KhuyenMaiCodes",
                newName: "IX_KhuyenMaiCodes_KhuyenMaiId");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonKhuyenMai_KhuyenMaiId",
                table: "HoaDonKhuyenMais",
                newName: "IX_HoaDonKhuyenMais_KhuyenMaiId");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonKhuyenMai_HoaDonMaHd",
                table: "HoaDonKhuyenMais",
                newName: "IX_HoaDonKhuyenMais_HoaDonMaHd");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KhuyenMaiCodes",
                table: "KhuyenMaiCodes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KhuyenMais",
                table: "KhuyenMais",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HoaDonKhuyenMais",
                table: "HoaDonKhuyenMais",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonKhuyenMais_HoaDon_HoaDonMaHd",
                table: "HoaDonKhuyenMais",
                column: "HoaDonMaHd",
                principalTable: "HoaDon",
                principalColumn: "MaHD",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMais_KhuyenMaiId",
                table: "HoaDonKhuyenMais",
                column: "KhuyenMaiId",
                principalTable: "KhuyenMais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KhuyenMaiCodes_KhuyenMais_KhuyenMaiId",
                table: "KhuyenMaiCodes",
                column: "KhuyenMaiId",
                principalTable: "KhuyenMais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonKhuyenMais_HoaDon_HoaDonMaHd",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMais_KhuyenMaiId",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropForeignKey(
                name: "FK_KhuyenMaiCodes_KhuyenMais_KhuyenMaiId",
                table: "KhuyenMaiCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KhuyenMais",
                table: "KhuyenMais");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KhuyenMaiCodes",
                table: "KhuyenMaiCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HoaDonKhuyenMais",
                table: "HoaDonKhuyenMais");

            migrationBuilder.RenameTable(
                name: "KhuyenMais",
                newName: "KhuyenMai");

            migrationBuilder.RenameTable(
                name: "KhuyenMaiCodes",
                newName: "KhuyenMaiCode");

            migrationBuilder.RenameTable(
                name: "HoaDonKhuyenMais",
                newName: "HoaDonKhuyenMai");

            migrationBuilder.RenameIndex(
                name: "IX_KhuyenMaiCodes_KhuyenMaiId",
                table: "KhuyenMaiCode",
                newName: "IX_KhuyenMaiCode_KhuyenMaiId");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonKhuyenMais_KhuyenMaiId",
                table: "HoaDonKhuyenMai",
                newName: "IX_HoaDonKhuyenMai_KhuyenMaiId");

            migrationBuilder.RenameIndex(
                name: "IX_HoaDonKhuyenMais_HoaDonMaHd",
                table: "HoaDonKhuyenMai",
                newName: "IX_HoaDonKhuyenMai_HoaDonMaHd");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KhuyenMai",
                table: "KhuyenMai",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KhuyenMaiCode",
                table: "KhuyenMaiCode",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HoaDonKhuyenMai",
                table: "HoaDonKhuyenMai",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonKhuyenMai_HoaDon_HoaDonMaHd",
                table: "HoaDonKhuyenMai",
                column: "HoaDonMaHd",
                principalTable: "HoaDon",
                principalColumn: "MaHD",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonKhuyenMai_KhuyenMai_KhuyenMaiId",
                table: "HoaDonKhuyenMai",
                column: "KhuyenMaiId",
                principalTable: "KhuyenMai",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KhuyenMaiCode_KhuyenMai_KhuyenMaiId",
                table: "KhuyenMaiCode",
                column: "KhuyenMaiId",
                principalTable: "KhuyenMai",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
