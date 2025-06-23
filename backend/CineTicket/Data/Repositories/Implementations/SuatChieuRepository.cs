using CineTicket.Data;
using CineTicket.Data.Repositories.Interfaces;
using CineTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class SuatChieuRepository : ISuatChieuRepository
    {
        private readonly CineTicketDbContext _context;

        public SuatChieuRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SuatChieu>> GetAllAsync()
        {
            return await _context.SuatChieus
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPhongNavigation)
                .Include(s => s.Ves)
                .ToListAsync();
        }

        public async Task<SuatChieu?> GetByIdAsync(int id)
        {
            return await _context.SuatChieus
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPhongNavigation)
                .Include(s => s.Ves)
                .FirstOrDefaultAsync(s => s.MaSuat == id);
        }

        public async Task<List<SuatChieu>> GetByPhimIdAsync(int maPhim)
        {
            var currentTime = DateTime.Now;

            return await _context.SuatChieus
                .Where(s => s.MaPhim == maPhim && s.ThoiGianBatDau > currentTime)
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPhongNavigation)
                .Include(s => s.Ves)
                .ToListAsync();
        }


        public async Task<SuatChieu> CreateAsync(SuatChieu suatChieu)
        {
            _context.SuatChieus.Add(suatChieu);
            await _context.SaveChangesAsync();
            return suatChieu;
        }

        public async Task<bool> UpdateAsync(SuatChieu suatChieu)
        {
            _context.SuatChieus.Update(suatChieu);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var suat = await _context.SuatChieus.FindAsync(id);
            if (suat == null) return false;
            _context.SuatChieus.Remove(suat);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
