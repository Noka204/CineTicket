using System.ComponentModel.DataAnnotations.Schema;

namespace CineTicket.DTOs.ChiTietHoaDon
{
    public class CreateChiTietHoaDonDTO
    {
        public int? MaVe { get; set; }
        public int? MaBn { get; set; }
        public int SoLuong { get; set; }      // không nullable
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }   // không nullable

    }
}
