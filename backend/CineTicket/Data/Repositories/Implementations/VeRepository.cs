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
            => await _context.Ves.AsNoTracking().ToListAsync();

        public async Task<Ve?> GetByIdAsync(int id)
            => await _context.Ves.FindAsync(id);

        public async Task<Ve> CreateAsync(Ve model)
        {
            _context.Ves.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> UpdateAsync(Ve model)
        {
            _context.Ves.Update(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _context.Ves.FindAsync(id);
            if (v == null) return false;
            _context.Ves.Remove(v);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<Ve?> GetByGheAndSuatAsync(int maGhe, int maSuat)
            => _context.Ves.FirstOrDefaultAsync(v => v.MaGhe == maGhe && v.MaSuat == maSuat);

        // NEW
        public async Task<IEnumerable<Ve>> GetHoldsByUserAsync(string userId, int? maSuat = null)
        {
            var now = DateTime.Now;
            var q = _context.Ves.AsNoTracking()
                .Where(v => v.TrangThai == "TamGiu"
                            && v.NguoiGiuId == userId
                            && v.ThoiGianTamGiu != null
                            && v.ThoiGianTamGiu > now);

            if (maSuat.HasValue) q = q.Where(v => v.MaSuat == maSuat.Value);

            return await q.ToListAsync();
        }
    }
}
