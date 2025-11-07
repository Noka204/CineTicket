using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineTicket.Models
{
    public partial class HoaDon
    {
        public int MaHd { get; set; }
        public DateTime? NgayLap { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }
        public string? ClientToken { get; set; }
        public int? MaSuat { get; set; }
        public string? HinhThucThanhToan { get; set; }
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        // === THÊM TỐI THIỂU: tổng tiền đã giảm bởi khuyến mãi ===
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTienKhuyenMai { get; set; } = 0m;

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

        // nav tới bảng nối
        public virtual ICollection<HoaDonKhuyenMai> HoaDonKhuyenMais { get; set; } = new List<HoaDonKhuyenMai>();
    }
}
