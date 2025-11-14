using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineTicket.Migrations
{
    /// <inheritdoc />
    public partial class khuyenmai__1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "KhuyenMais",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxGlobalRedemptions",
                table: "KhuyenMais",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderAmount",
                table: "KhuyenMais",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "MaxGlobalRedemptions",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "MinOrderAmount",
                table: "KhuyenMais");
        }
    }
}
