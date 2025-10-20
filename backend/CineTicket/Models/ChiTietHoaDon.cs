using CineTicket.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public partial class ChiTietHoaDon
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaCthd { get; set; }

    public int MaHd { get; set; }       // luôn phải có hóa đơn
    public int? MaVe { get; set; }
    public int? MaBn { get; set; }
    public int? MaRap { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SoLuong { get; set; }    // >=1, không nullable

    [Column(TypeName = "decimal(18,2)")]
    public decimal DonGia { get; set; } // không nullable, luôn có giá

    public virtual BapNuoc? MaBnNavigation { get; set; }
    public virtual HoaDon MaHdNavigation { get; set; } = null!;
    public virtual Ve? MaVeNavigation { get; set; }
    public virtual Rap? MaRapNavigation { get; set; }
}
