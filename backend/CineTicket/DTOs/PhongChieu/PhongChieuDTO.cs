﻿namespace CineTicket.DTOs
{
    public class PhongChieuDTO
    {
        public int MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public int? SoGhe { get; set; }
        public int MaRap { get; internal set; }
        public string? TenRap { get; internal set; }
    }
}
