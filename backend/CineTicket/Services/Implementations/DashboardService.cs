using CineTicket.Data;
using CineTicket.DTOs.HoaDon.CineTicket.DTOs.Dashboard;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;
        private readonly CineTicketDbContext _db;

        public DashboardService(IDashboardRepository repository, CineTicketDbContext db)
        {
            _repository = repository;
            _db = db;
        }

        public async Task<List<DailyRevenueDTO>> GetDoanhThu30NgayGanNhatAsync(bool chiTinhDaThanhToan = true)
        {
            var today = DateTime.Today;          // ví dụ: 2025-08-18 00:00:00
            var start = today.AddDays(-29);      // lấy đủ 30 ngày: today và 29 ngày trước
            var end = today.AddDays(1);        // < end (đầu ngày kế tiếp)

            var query = _db.HoaDons
                .AsNoTracking()
                .Where(h => h.NgayLap.HasValue &&
                            h.NgayLap.Value >= start &&
                            h.NgayLap.Value < end);

            if (chiTinhDaThanhToan)
                query = query.Where(h => h.TrangThai == "Đã thanh toán");

            // group theo ngày (date only), sum TongTien (null => 0)
            var grouped = await query
                .GroupBy(h => h.NgayLap!.Value.Date)
                .Select(g => new DailyRevenueDTO
                {
                    Ngay = g.Key,
                    TongTien = g.Sum(x => x.TongTien ?? 0)
                })
                .ToListAsync();

            return grouped;
        }
    }
}
