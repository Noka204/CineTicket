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

        public DiscountKind LoaiGiam { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MucGiam { get; set; }        // 10 -> 10% khi Percent; 50000 khi Amount

        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }

        public ICollection<KhuyenMaiCode> Codes { get; set; } = new List<KhuyenMaiCode>();
        public ICollection<HoaDonKhuyenMai> Redemptions { get; set; } = new List<HoaDonKhuyenMai>();
    }
}
