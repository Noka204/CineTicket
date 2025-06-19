using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class addapplicationuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BapNuoc",
                columns: table => new
                {
                    MaBN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBN = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gia = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BapNuoc__272475AD32F5D274", x => x.MaBN);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHD = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayLap = table.Column<DateTime>(type: "datetime", nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    HinhThucThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HoaDon__2725A6E0DE25F710", x => x.MaHD);
                });

            migrationBuilder.CreateTable(
                name: "LoaiPhim",
                columns: table => new
                {
                    MaLoaiPhim = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiPhim = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoaiPhim__9CA05BEFEB048A14", x => x.MaLoaiPhim);
                });

            migrationBuilder.CreateTable(
                name: "PhongChieu",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoGhe = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhongChi__20BD5E5BD2124FD6", x => x.MaPhong);
                });

            migrationBuilder.CreateTable(
                name: "Phim",
                columns: table => new
                {
                    MaPhim = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenPhim = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ThoiLuong = table.Column<int>(type: "int", nullable: true),
                    DaoDien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Poster = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaLoaiPhim = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Phim__4AC03DE3772452C4", x => x.MaPhim);
                    table.ForeignKey(
                        name: "FK__Phim__MaLoaiPhim__398D8EEE",
                        column: x => x.MaLoaiPhim,
                        principalTable: "LoaiPhim",
                        principalColumn: "MaLoaiPhim");
                });

            migrationBuilder.CreateTable(
                name: "Ghe",
                columns: table => new
                {
                    MaGhe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoGhe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LoaiGhe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaPhong = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ghe__3CD3C67BFCA14E9D", x => x.MaGhe);
                    table.ForeignKey(
                        name: "FK__Ghe__MaPhong__4222D4EF",
                        column: x => x.MaPhong,
                        principalTable: "PhongChieu",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "SuatChieu",
                columns: table => new
                {
                    MaSuat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhim = table.Column<int>(type: "int", nullable: true),
                    MaPhong = table.Column<int>(type: "int", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime", nullable: true),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime", nullable: true),
                    NgayChieu = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SuatChie__A69D0241FFB40570", x => x.MaSuat);
                    table.ForeignKey(
                        name: "FK__SuatChieu__MaPhi__3E52440B",
                        column: x => x.MaPhim,
                        principalTable: "Phim",
                        principalColumn: "MaPhim");
                    table.ForeignKey(
                        name: "FK__SuatChieu__MaPho__3F466844",
                        column: x => x.MaPhong,
                        principalTable: "PhongChieu",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "Ve",
                columns: table => new
                {
                    MaVe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGhe = table.Column<int>(type: "int", nullable: true),
                    MaSuat = table.Column<int>(type: "int", nullable: true),
                    GiaVe = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ve__2725100F335089D2", x => x.MaVe);
                    table.ForeignKey(
                        name: "FK__Ve__MaGhe__44FF419A",
                        column: x => x.MaGhe,
                        principalTable: "Ghe",
                        principalColumn: "MaGhe");
                    table.ForeignKey(
                        name: "FK__Ve__MaSuat__45F365D3",
                        column: x => x.MaSuat,
                        principalTable: "SuatChieu",
                        principalColumn: "MaSuat");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDon",
                columns: table => new
                {
                    MaCTHD = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHD = table.Column<int>(type: "int", nullable: true),
                    MaVe = table.Column<int>(type: "int", nullable: true),
                    MaBN = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChiTietH__1E4FA7715A64D0C4", x => x.MaCTHD);
                    table.ForeignKey(
                        name: "FK__ChiTietHoa__MaBN__4E88ABD4",
                        column: x => x.MaBN,
                        principalTable: "BapNuoc",
                        principalColumn: "MaBN");
                    table.ForeignKey(
                        name: "FK__ChiTietHoa__MaHD__4CA06362",
                        column: x => x.MaHD,
                        principalTable: "HoaDon",
                        principalColumn: "MaHD");
                    table.ForeignKey(
                        name: "FK__ChiTietHoa__MaVe__4D94879B",
                        column: x => x.MaVe,
                        principalTable: "Ve",
                        principalColumn: "MaVe");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaBN",
                table: "ChiTietHoaDon",
                column: "MaBN");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaHD",
                table: "ChiTietHoaDon",
                column: "MaHD");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaVe",
                table: "ChiTietHoaDon",
                column: "MaVe");

            migrationBuilder.CreateIndex(
                name: "IX_Ghe_MaPhong",
                table: "Ghe",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Phim_MaLoaiPhim",
                table: "Phim",
                column: "MaLoaiPhim");

            migrationBuilder.CreateIndex(
                name: "IX_SuatChieu_MaPhim",
                table: "SuatChieu",
                column: "MaPhim");

            migrationBuilder.CreateIndex(
                name: "IX_SuatChieu_MaPhong",
                table: "SuatChieu",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaGhe",
                table: "Ve",
                column: "MaGhe");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaSuat",
                table: "Ve",
                column: "MaSuat");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietHoaDon");

            migrationBuilder.DropTable(
                name: "BapNuoc");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "Ve");

            migrationBuilder.DropTable(
                name: "Ghe");

            migrationBuilder.DropTable(
                name: "SuatChieu");

            migrationBuilder.DropTable(
                name: "Phim");

            migrationBuilder.DropTable(
                name: "PhongChieu");

            migrationBuilder.DropTable(
                name: "LoaiPhim");
        }
    }
}
