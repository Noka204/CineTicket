using System;
using System.Collections.Generic;

namespace CineTicket.Models
{
    public partial class Ve
    {
        public int MaVe { get; set; }

        public int? MaGhe { get; set; }

        public int? MaSuat { get; set; }

        public decimal? GiaVe { get; set; }

        public string? TrangThai { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

        public virtual Ghe? MaGheNavigation { get; set; }

        public virtual SuatChieu? MaSuatNavigation { get; set; }
    }

}
