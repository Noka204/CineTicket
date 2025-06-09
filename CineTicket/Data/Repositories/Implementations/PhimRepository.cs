using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class PhimRepository : IPhimRepository
    {
        private readonly CineTicketDbContext _context;

        public PhimRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<List<Phim>> GetAllAsync()
        {
            return await _context.Phims
                .Include(p => p.MaLoaiPhimNavigation)
                .Include(p => p.SuatChieus)
                .ToListAsync();
        }

        public async Task<Phim?> GetByIdAsync(int id)
        {
            return await _context.Phims
                .Include(p => p.MaLoaiPhimNavigation)
                .Include(p => p.SuatChieus)
                .FirstOrDefaultAsync(p => p.MaPhim == id);
        }

        public async Task<Phim> CreateAsync(Phim phim)
        {
            _context.Phims.Add(phim);
            await _context.SaveChangesAsync();
            return phim;
        }


        public async Task<bool> UpdateAsync(Phim phim)
        {
            _context.Phims.Update(phim);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var phim = await _context.Phims.FindAsync(id);
            if (phim == null) return false;

            _context.Phims.Remove(phim);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
