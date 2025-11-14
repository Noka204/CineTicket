using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineTicket.Models
{
    public class KhuyenMai
    {
        public int Id { get; set; }
        public string Ten { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public string? Code { get; set; }                 // ma dung chung (co the null)
        public DiscountKind LoaiGiam { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MucGiam { get; set; }              // 10 -> 10% neu PhanTram; 50000 neu SoTien

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; }      // toi thieu ap dung (optional)

        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }

        public int? MaxGlobalRedemptions { get; set; }    // gioi han tong luot (optional)

        public ICollection<KhuyenMaiCode> Codes { get; set; } = new List<KhuyenMaiCode>();
        public ICollection<HoaDonKhuyenMai> Redemptions { get; set; } = new List<HoaDonKhuyenMai>();
    }
}
