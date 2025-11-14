using CineTicket.Models;
using System;

namespace CineTicket.DTOs.KhuyenMai
{
    public class KhuyenMaiValidateResultDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }

        public int? KhuyenMaiId { get; set; }
        public string? Ten { get; set; }
        public DiscountKind LoaiGiam { get; set; }
        public decimal MucGiam { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }

        // null nếu là KM toàn cục (không dùng code riêng)
        public string? CodeUsed { get; set; }
    }
}
