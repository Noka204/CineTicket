
using CineTicket.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Rap", Schema = "dbo")] // ⟵ map thẳng vào bảng thực tế
public partial class Rap
{
    [Key]
    public int MaRap { get; set; }
    public string? TenRap { get; set; }
    public string? DiaChi { get; set; }
    public string? ThanhPho { get; set; }
    public bool? HoatDong { get; set; } = true;
    public virtual ICollection<PhongChieu> PhongChieus { get; set; } = new HashSet<PhongChieu>();
    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new HashSet<ChiTietHoaDon>();
}
