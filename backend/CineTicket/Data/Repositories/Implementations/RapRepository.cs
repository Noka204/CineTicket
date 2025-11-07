using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class RapRepository : IRapRepository
    {
        private readonly CineTicketDbContext _context;
        public RapRepository(CineTicketDbContext db) => _context = db;

        // ========== CRUD cơ bản ==========
        public Task<List<Rap>> GetAllAsync()
            => _context.Raps
                .AsNoTracking()
                .OrderBy(x => x.MaRap)
                .ToListAsync();

        public Task<Rap?> GetByIdAsync(int id)
            => _context.Raps
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaRap == id);

        public async Task<Rap> AddAsync(Rap entity)
        {
            _context.Raps.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Rap?> UpdateAsync(Rap entity)
        {
            var exists = await _context.Raps.AnyAsync(x => x.MaRap == entity.MaRap);
            if (!exists) return null;

            _context.Raps.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var found = await _context.Raps.FindAsync(id);
            if (found is null) return false;

            _context.Raps.Remove(found);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> ExistsAsync(int id)
            => _context.Raps.AnyAsync(x => x.MaRap == id);

        // ========== Tiện ích (Cities & ByCity) ==========

        // Trả danh sách thành phố đã chuẩn hóa:
        // - bỏ null/blank, trim,
        // - gộp trùng không phân biệt hoa/thường (LOWER) và khoảng trắng,
        // - giữ lại 1 "bản gốc" có format đẹp nhất theo Min (ổn cho UI).
        public async Task<List<string>> GetCitiesAsync()
        {
            return await _context.Raps
                .AsNoTracking()
                .Where(r => r.HoatDong == true)
                .Where(r => !string.IsNullOrWhiteSpace(r.ThanhPho))
                .Select(r => new
                {
                    Raw = r.ThanhPho!,                         // ví dụ: "Hồ Chí Minh"
                    Norm = r.ThanhPho!.Trim().ToLower()        // ví dụ: "hồ chí minh"
                })
                .GroupBy(x => x.Norm)
                .Select(g => g.Min(x => x.Raw)!)               // lấy 1 định dạng readable
                .OrderBy(c => c)
                .ToListAsync();
        }

        // Lấy rạp theo thành phố, so khớp city theo cách:
        // Trim + ToLower() để không phân biệt hoa/thường & khoảng trắng
        public async Task<List<Rap>> GetByCityAsync(string thanhPho)
        {
            if (string.IsNullOrWhiteSpace(thanhPho))
                return new List<Rap>();

            var norm = thanhPho.Trim().ToLower();

            return await _context.Raps
                .AsNoTracking()
                .Where(r => r.HoatDong == true && r.ThanhPho != null)
                .Where(r => r.ThanhPho!.Trim().ToLower() == norm)
                .OrderBy(r => r.TenRap)
                .ToListAsync();
        }
    }
}
