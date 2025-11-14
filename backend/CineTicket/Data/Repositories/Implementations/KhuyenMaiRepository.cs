// ==============================
// File: Repositories/Implementations/KhuyenMaiRepository.cs
// ==============================
using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class KhuyenMaiRepository : IKhuyenMaiRepository
    {
        private readonly CineTicketDbContext _db;
        public KhuyenMaiRepository(CineTicketDbContext db) => _db = db;

        public Task<List<KhuyenMai>> GetAllAsync() =>
            _db.KhuyenMais
               .Include(x => x.Codes) // để đếm SoCode khi map DTO
               .OrderByDescending(x => x.Id)
               .ToListAsync();

        public Task<KhuyenMai?> GetAsync(int id) =>
            _db.KhuyenMais.Include(x => x.Codes).FirstOrDefaultAsync(x => x.Id == id)!;

        public async Task AddAsync(KhuyenMai entity)
        {
            _db.KhuyenMais.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(KhuyenMai entity)
        {
            _db.KhuyenMais.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var found = await _db.KhuyenMais.FindAsync(id);
            if (found is null) return false;

            _db.KhuyenMais.Remove(found);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
