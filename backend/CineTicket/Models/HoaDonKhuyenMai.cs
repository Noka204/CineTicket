using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineTicket.Models
{
    public class HoaDonKhuyenMai
    {
        public int Id { get; set; }

        // bat buoc
        public int MaHd { get; set; }                      // FK -> HoaDon.MaHd

        public int? KhuyenMaiId { get; set; }              // FK -> KhuyenMai.Id
        public int? KhuyenMaiCodeId { get; set; }          // FK -> KhuyenMaiCode.Id

        public string? Code { get; set; }

        public string? UserId { get; set; }

        public DiscountKind LoaiGiam { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MucGiam { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaTriGiam { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // nav
        public virtual HoaDon HoaDon { get; set; } = null!;
        public virtual KhuyenMai? KhuyenMai { get; set; }
        public virtual KhuyenMaiCode? KhuyenMaiCode { get; set; }
    }
}
