using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineTicket.Models;
[Table("BapNuoc")]
public partial class BapNuoc
{
    public int MaBn { get; set; }

    public string? TenBn { get; set; }

    public decimal? Gia { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
}
