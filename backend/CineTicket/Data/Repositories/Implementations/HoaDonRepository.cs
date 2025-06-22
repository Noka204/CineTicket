using CineTicket.Models;
using CineTicket.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Data.Repositories.Implementations
{
    public class HoaDonRepository : IHoaDonRepository
    {
        private readonly CineTicketDbContext _context;
        public HoaDonRepository(CineTicketDbContext context) => _context = context;

        public async Task<IEnumerable<HoaDon>> GetAllAsync() => await _context.HoaDons.ToListAsync();

        public async Task<HoaDon?> GetByIdAsync(int id) => await _context.HoaDons.FindAsync(id);

        public async Task<HoaDon> CreateAsync(HoaDon model)
        {
            _context.HoaDons.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> UpdateAsync(HoaDon model)
        {
            var existing = await _context.HoaDons.FindAsync(model.MaHd);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteWithDetailsAsync(int id)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hoaDon == null) return false;

            _context.ChiTietHoaDons.RemoveRange(hoaDon.ChiTietHoaDons);
            _context.HoaDons.Remove(hoaDon);

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
