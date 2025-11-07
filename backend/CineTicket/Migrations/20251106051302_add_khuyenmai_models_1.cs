using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class add_khuyenmai_models_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TongTienKhuyenMai",
                table: "HoaDon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "KhuyenMai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LoaiGiam = table.Column<int>(type: "int", nullable: false),
                    MucGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KetThuc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMai", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonKhuyenMai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHd = table.Column<int>(type: "int", nullable: false),
                    HoaDonMaHd = table.Column<int>(type: "int", nullable: false),
                    KhuyenMaiId = table.Column<int>(type: "int", nullable: false),
                    CodeDaDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiamThucTe = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonKhuyenMai", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoaDonKhuyenMai_HoaDon_HoaDonMaHd",
                        column: x => x.HoaDonMaHd,
                        principalTable: "HoaDon",
                        principalColumn: "MaHD",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDonKhuyenMai_KhuyenMai_KhuyenMaiId",
                        column: x => x.KhuyenMaiId,
                        principalTable: "KhuyenMai",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMaiCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KhuyenMaiId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMaiCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhuyenMaiCode_KhuyenMai_KhuyenMaiId",
                        column: x => x.KhuyenMaiId,
                        principalTable: "KhuyenMai",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonKhuyenMai_HoaDonMaHd",
                table: "HoaDonKhuyenMai",
                column: "HoaDonMaHd");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonKhuyenMai_KhuyenMaiId",
                table: "HoaDonKhuyenMai",
                column: "KhuyenMaiId");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMaiCode_KhuyenMaiId",
                table: "KhuyenMaiCode",
                column: "KhuyenMaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoaDonKhuyenMai");

            migrationBuilder.DropTable(
                name: "KhuyenMaiCode");

            migrationBuilder.DropTable(
                name: "KhuyenMai");

            migrationBuilder.DropColumn(
                name: "TongTienKhuyenMai",
                table: "HoaDon");
        }
    }
}
