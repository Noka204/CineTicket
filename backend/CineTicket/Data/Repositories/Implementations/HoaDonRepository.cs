using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class HoaDonRepository : IHoaDonRepository
    {
        private readonly CineTicketDbContext _context;

        public HoaDonRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<HoaDon> CreateAsync(HoaDon hoaDon)
        {
            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();
            return hoaDon;
        }

        public async Task<HoaDon?> GetByIdAsync(int id)
        {
            return await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(c => c.MaVeNavigation)
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(c => c.MaBnNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == id);
        }

        public async Task<IEnumerable<HoaDon>> GetAllAsync()
        {
            return await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(c => c.MaVeNavigation)
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(c => c.MaBnNavigation)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(HoaDon hoaDon)
        {
            var existing = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.MaHd == hoaDon.MaHd);

            if (existing == null) return false;

            existing.NgayLap = hoaDon.NgayLap;
            existing.TongTien = hoaDon.TongTien;
            existing.TrangThai = hoaDon.TrangThai;
            existing.HinhThucThanhToan = hoaDon.HinhThucThanhToan;

            _context.ChiTietHoaDons.RemoveRange(existing.ChiTietHoaDons);
            existing.ChiTietHoaDons = hoaDon.ChiTietHoaDons;

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