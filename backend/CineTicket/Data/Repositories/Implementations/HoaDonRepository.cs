// ==============================
// File: Repositories/Implementations/HoaDonRepository.cs
// ==============================
using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class HoaDonRepository : IHoaDonRepository
    {
        private readonly CineTicketDbContext _context;
        private readonly ILogger<HoaDonRepository> _logger;

        public HoaDonRepository(CineTicketDbContext context, ILogger<HoaDonRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HoaDon> CreateAsync(HoaDon hoaDon)
        {
            await _context.HoaDons.AddAsync(hoaDon);
            await _context.SaveChangesAsync();
            return hoaDon;
        }

        public Task<HoaDon?> FindByClientTokenAsync(string userId, string clientToken)
            => _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.ApplicationUserId == userId && h.ClientToken == clientToken);

        public Task<HoaDon?> GetByIdAsync(int maHd)
            => _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(c => c.MaVeNavigation)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(c => c.MaBnNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == maHd);

        public async Task<IEnumerable<HoaDon>> GetAllAsync()
        {
            return await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(c => c.MaVeNavigation)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(c => c.MaBnNavigation)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(HoaDon hoaDon)
        {
            var existing = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.MaHd == hoaDon.MaHd);

            if (existing == null) return false;

            // cập nhật các trường cho phép
            existing.TrangThai = hoaDon.TrangThai;
            existing.HinhThucThanhToan = hoaDon.HinhThucThanhToan;
            existing.TongTien = hoaDon.TongTien;
            existing.NgayLap = hoaDon.NgayLap;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hoaDon == null) return false;

            _context.ChiTietHoaDons.RemoveRange(hoaDon.ChiTietHoaDons);
            _context.HoaDons.Remove(hoaDon);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
