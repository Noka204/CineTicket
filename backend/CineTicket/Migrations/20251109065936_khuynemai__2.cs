using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class khuynemai__2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMais_KhuyenMaiId",
                table: "HoaDonKhuyenMais");

            migrationBuilder.RenameColumn(
                name: "GiamThucTe",
                table: "HoaDonKhuyenMais",
                newName: "MucGiam");

            migrationBuilder.RenameColumn(
                name: "CodeDaDung",
                table: "HoaDonKhuyenMais",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "KhuyenMaiId",
                table: "HoaDonKhuyenMais",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "HoaDonKhuyenMais",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HoaDonKhuyenMais",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "GiaTriGiam",
                table: "HoaDonKhuyenMais",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "KhuyenMaiCodeId",
                table: "HoaDonKhuyenMais",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoaiGiam",
                table: "HoaDonKhuyenMais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonKhuyenMais_KhuyenMaiCodeId",
                table: "HoaDonKhuyenMais",
                column: "KhuyenMaiCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMaiCodes_KhuyenMaiCodeId",
                table: "HoaDonKhuyenMais",
                column: "KhuyenMaiCodeId",
                principalTable: "KhuyenMaiCodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMais_KhuyenMaiId",
                table: "HoaDonKhuyenMais",
                column: "KhuyenMaiId",
                principalTable: "KhuyenMais",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMaiCodes_KhuyenMaiCodeId",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMais_KhuyenMaiId",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropIndex(
                name: "IX_HoaDonKhuyenMais_KhuyenMaiCodeId",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropColumn(
                name: "GiaTriGiam",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropColumn(
                name: "KhuyenMaiCodeId",
                table: "HoaDonKhuyenMais");

            migrationBuilder.DropColumn(
                name: "LoaiGiam",
                table: "HoaDonKhuyenMais");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "HoaDonKhuyenMais",
                newName: "CodeDaDung");

            migrationBuilder.RenameColumn(
                name: "MucGiam",
                table: "HoaDonKhuyenMais",
                newName: "GiamThucTe");

            migrationBuilder.AlterColumn<int>(
                name: "KhuyenMaiId",
                table: "HoaDonKhuyenMais",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDonKhuyenMais_KhuyenMais_KhuyenMaiId",
                table: "HoaDonKhuyenMais",
                column: "KhuyenMaiId",
                principalTable: "KhuyenMais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
