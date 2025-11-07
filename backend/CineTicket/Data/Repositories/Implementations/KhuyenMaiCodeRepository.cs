using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class KhuyenMaiCodeRepository : IKhuyenMaiCodeRepository
    {
        private readonly CineTicketDbContext _db;
        public KhuyenMaiCodeRepository(CineTicketDbContext db) => _db = db;

        public Task<KhuyenMaiCode?> GetByIdAsync(int id) =>
            _db.KhuyenMaiCodes.Include(c => c.KhuyenMai).FirstOrDefaultAsync(x => x.Id == id)!;

        public Task<KhuyenMaiCode?> GetByCodeAsync(string code)
        {
            var norm = code.Trim().ToUpperInvariant();
            return _db.KhuyenMaiCodes.Include(c => c.KhuyenMai)
                .FirstOrDefaultAsync(x => x.Code == norm)!;
        }

        public Task<List<KhuyenMaiCode>> GetByPromotionAsync(int khuyenMaiId) =>
            _db.KhuyenMaiCodes.Where(x => x.KhuyenMaiId == khuyenMaiId)
               .OrderByDescending(x => x.Id).ToListAsync();

        public async Task AddAsync(KhuyenMaiCode code)
        {
            code.Code = code.Code.Trim().ToUpperInvariant();
            _db.KhuyenMaiCodes.Add(code);
            await _db.SaveChangesAsync();
        }

        public async Task AddBulkAsync(IEnumerable<KhuyenMaiCode> codes)
        {
            foreach (var c in codes) c.Code = c.Code.Trim().ToUpperInvariant();
            await _db.KhuyenMaiCodes.AddRangeAsync(codes);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var found = await _db.KhuyenMaiCodes.FindAsync(id);
            if (found is null) return false;
            _db.KhuyenMaiCodes.Remove(found);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
