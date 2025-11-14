using CineTicket.Models;

namespace CineTicket.DTOs.KhuyenMai
{
    public class KhuyenMaiDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = null!;
        public bool IsActive { get; set; }
        public DiscountKind LoaiGiam { get; set; }
        public decimal MucGiam { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
    }

}
