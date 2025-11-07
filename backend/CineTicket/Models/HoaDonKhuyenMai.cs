using System.ComponentModel.DataAnnotations.Schema;

namespace CineTicket.Models
{
    public class HoaDonKhuyenMai
    {
        // FK tới HoaDon dùng khóa hiện có MaHd
        public int Id { get; set; }               // PK
        public int MaHd { get; set; }
        public HoaDon HoaDon { get; set; } = null!;

        public int KhuyenMaiId { get; set; }
        public KhuyenMai KhuyenMai { get; set; } = null!;

        public string? CodeDaDung { get; set; }           // nếu dùng code cụ thể

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiamThucTe { get; set; }           // số tiền thực trừ (đã tính)
    }
}
