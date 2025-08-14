using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class PhongChieuRepository : IPhongChieuRepository
    {
        private readonly CineTicketDbContext _context;

        public PhongChieuRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhongChieu>> GetAllAsync()
        {
            return await _context.PhongChieus
                .Include(p => p.Ghes)
                .Include(p => p.SuatChieus)
                .ToListAsync();
        }

        public async Task<PhongChieu?> GetByIdAsync(int id)
        {
            return await _context.PhongChieus
                .Include(p => p.Ghes)
                .Include(p => p.SuatChieus)
                .FirstOrDefaultAsync(p => p.MaPhong == id);
        }

        public async Task<PhongChieu> CreateAsync(PhongChieu phongChieu)
        {
            _context.PhongChieus.Add(phongChieu);
            await _context.SaveChangesAsync();
            return phongChieu;
        }

        public async Task<bool> UpdateAsync(PhongChieu phongChieu)
        {
            _context.PhongChieus.Update(phongChieu);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var phong = await _context.PhongChieus.FindAsync(id);
            if (phong == null) return false;

            var ghes = _context.Ghes.Where(g => g.MaPhong == id);
            _context.Ghes.RemoveRange(ghes);

            _context.PhongChieus.Remove(phong);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
