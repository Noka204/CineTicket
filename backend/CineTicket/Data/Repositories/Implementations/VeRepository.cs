using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class VeRepository : IVeRepository
    {
        private readonly CineTicketDbContext _context;

        public VeRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ve>> GetAllAsync()
        {
            return await _context.Ves
                .Include(v => v.MaGheNavigation)
                .Include(v => v.MaSuatNavigation)
                .ToListAsync();
        }

        public async Task<Ve?> GetByIdAsync(int id)
        {
            return await _context.Ves
                .Include(v => v.MaGheNavigation)
                .Include(v => v.MaSuatNavigation)
                .FirstOrDefaultAsync(v => v.MaVe == id);
        }

        public async Task<Ve> CreateAsync(Ve ve)
        {
            _context.Ves.Add(ve);
            await _context.SaveChangesAsync();
            return ve;
        }

        public async Task<bool> UpdateAsync(Ve ve)
        {
            _context.Ves.Update(ve);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ve = await _context.Ves.FindAsync(id);
            if (ve == null) return false;
            _context.Ves.Remove(ve);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
