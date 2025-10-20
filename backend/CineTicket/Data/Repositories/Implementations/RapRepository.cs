using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories
{
    public class RapRepository : IRapRepository
    {
        private readonly CineTicketDbContext _context;
        public RapRepository(CineTicketDbContext db) => _context = db;

        public Task<List<Rap>> GetAllAsync()
            => _context.Raps.AsNoTracking().OrderBy(x => x.MaRap).ToListAsync();

        public Task<Rap?> GetByIdAsync(int id)
            => _context.Raps.AsNoTracking().FirstOrDefaultAsync(x => x.MaRap == id);

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

        // ====== THÊM MỚI ======
        public async Task<List<string>> GetCitiesAsync()
        {
            return await _context.Raps
                .AsNoTracking()
                .Where(r => r.HoatDong == true)                     // hoặc: .Where(r => r.HoatDong ?? false)
                .Where(r => !string.IsNullOrWhiteSpace(r.ThanhPho)) // tránh null/blank
                .Select(r => r.ThanhPho!.Trim())
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }


        public async Task<List<Rap>> GetByCityAsync(string thanhPho)
        {
            if (string.IsNullOrWhiteSpace(thanhPho)) return new List<Rap>();

            var city = thanhPho.Trim();

            return await _context.Raps
                .AsNoTracking()
                .Where(r => r.HoatDong == true                      // ✅ ép nullable -> bool
                            && r.ThanhPho != null
                            && r.ThanhPho == city)                  // so khớp đúng chính tả
                .OrderBy(r => r.TenRap)
                .ToListAsync();
        }

    }
}
