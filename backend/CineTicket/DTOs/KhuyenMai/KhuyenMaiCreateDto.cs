using CineTicket.Models;
using System;

namespace CineTicket.DTOs.KhuyenMai
{
    public class KhuyenMaiCreateDto
    {
        public string Ten { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DiscountKind LoaiGiam { get; set; }   // Percent | Amount
        public decimal MucGiam { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
    }
}