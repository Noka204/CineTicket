﻿using CineTicket.DTOs.ChiTietHoaDon;

namespace CineTicket.DTOs.HoaDon
{
    public class CreateHoaDonDTO
    {
        public DateTime NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public string HinhThucThanhToan { get; set; }
        public string? TrangThai { get; set; }
        public List<CreateChiTietHoaDonDTO> ChiTietHoaDons { get; set; }
    }
}
