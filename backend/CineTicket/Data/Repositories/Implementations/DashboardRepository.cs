using CineTicket.Data;
using CineTicket.Data.Repositories.Interfaces;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly CineTicketDbContext _context;

        public DashboardRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<List<(DateTime Ngay, decimal TongTien)>> GetDoanhThuTheoNgayTrongThang(int year, int month)
        {
            var data = await _context.HoaDons
                .Where(h => h.NgayLap.HasValue && h.NgayLap.Value.Year == year && h.NgayLap.Value.Month == month && h.TongTien.HasValue)
                .ToListAsync();

            var result = data
                .GroupBy(h => h.NgayLap!.Value.Date)
                .Select(g => (
                    Ngay: g.Key,
                    TongTien: g.Sum(h => h.TongTien ?? 0)
                ))
                .OrderBy(x => x.Ngay)
                .ToList();

            return result;
        }

    }
}
