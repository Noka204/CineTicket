using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class GheRepository : IGheRepository
    {
        private readonly CineTicketDbContext _context;

        public GheRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ghe>> GetAllAsync()
        {
            return await _context.Ghes
                .Include(g => g.MaPhongNavigation)
                .ToListAsync();
        }

        public async Task<Ghe?> GetByIdAsync(int id)
        {
            return await _context.Ghes
                .Include(g => g.MaPhongNavigation)
                .FirstOrDefaultAsync(g => g.MaGhe == id);
        }
        public async Task<IEnumerable<Ghe>> GetByPhongAsync(int maPhong)
        {
            return await _context.Ghes
                .Where(g => g.MaPhong == maPhong)
                .Include(g => g.MaPhongNavigation)
                .Include(g => g.Ves) // ✅ Thêm dòng này để lấy trạng thái vé
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<Ghe> CreateAsync(Ghe ghe)
        {
            _context.Ghes.Add(ghe);
            await _context.SaveChangesAsync();
            return ghe;
        }

        public async Task<bool> UpdateAsync(Ghe ghe)
        {
            _context.Ghes.Update(ghe);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ghe = await _context.Ghes.FindAsync(id);
            if (ghe == null) return false;
            _context.Ghes.Remove(ghe);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
