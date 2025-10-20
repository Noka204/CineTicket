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
                .AsNoTracking()
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPhongNavigation)!.ThenInclude(p => p!.Raps)
                .Include(s => s.Ves)
                .OrderBy(s => s.ThoiGianBatDau)
                .ToListAsync();
        }

        public async Task<SuatChieu?> GetByIdAsync(int id)
        {
            return await _context.SuatChieus
                .AsNoTracking()
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPhongNavigation)!.ThenInclude(p => p!.Raps)
                .Include(s => s.Ves)
                .FirstOrDefaultAsync(s => s.MaSuat == id);
        }

        public async Task<List<SuatChieu>> GetByPhimAsync(
            int maPhim,
            int? maRap = null,
            int? maPhong = null,
            DateOnly? ngay = null)
        {
            var now = DateTime.Now;

            var query = _context.SuatChieus
                .AsNoTracking()
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPhongNavigation)!.ThenInclude(p => p!.Raps)
                .Include(s => s.Ves)
                .Where(s => s.MaPhim == maPhim && s.ThoiGianBatDau > now)
                .AsQueryable();

            if (maPhong.HasValue)
            {
                query = query.Where(s => s.MaPhong == maPhong.Value);
            }

            if (maRap.HasValue)
            {
                // lọc qua navigation Rap của phòng
                query = query.Where(s => s.MaPhongNavigation != null
                                         && s.MaPhongNavigation.Raps != null
                                         && s.MaPhongNavigation.Raps.MaRap == maRap.Value);
            }

            if (ngay.HasValue)
            {
                // chuyển DateOnly -> khoảng DateTime của ngày đó (00:00:00 -> 23:59:59.9999999)
                var start = ngay.Value.ToDateTime(TimeOnly.MinValue);
                var end = ngay.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(s => s.ThoiGianBatDau >= start && s.ThoiGianBatDau <= end);
            }

            return await query
                .OrderBy(s => s.ThoiGianBatDau)
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
