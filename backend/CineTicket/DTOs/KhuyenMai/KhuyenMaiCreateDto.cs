// ==============================
// File: DTOs/KhuyenMai/KhuyenMaiCreateDto.cs
// ==============================
using CineTicket.Models;
using System.ComponentModel.DataAnnotations;

namespace CineTicket.DTOs.KhuyenMai
{
    public class KhuyenMaiCreateDto
    {
        [Required] public string Ten { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        [Required] public DiscountKind LoaiGiam { get; set; }
        [Range(0, double.MaxValue)] public decimal MucGiam { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
    }
}
