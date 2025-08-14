using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class LoaiPhimRepository : ILoaiPhimRepository
    {
        private readonly CineTicketDbContext _context;

        public LoaiPhimRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoaiPhim>> GetAllAsync() => await _context.LoaiPhims.ToListAsync();

        public async Task<LoaiPhim?> GetByIdAsync(int id) => await _context.LoaiPhims.FindAsync(id);

        public async Task<LoaiPhim> CreateAsync(LoaiPhim loaiPhim)
        {
            _context.LoaiPhims.Add(loaiPhim);
            await _context.SaveChangesAsync();
            return loaiPhim;
        }

        public async Task<bool> UpdateAsync(LoaiPhim loaiPhim)
        {
            _context.LoaiPhims.Update(loaiPhim);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.LoaiPhims.FindAsync(id);
            if (entity == null) return false;
            _context.LoaiPhims.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}