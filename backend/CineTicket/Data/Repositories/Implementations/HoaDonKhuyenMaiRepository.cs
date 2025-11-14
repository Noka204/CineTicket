using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Repositories.Implementations
{
    public class HoaDonKhuyenMaiRepository : IHoaDonKhuyenMaiRepository
    {
        private readonly CineTicketDbContext _db;
        public HoaDonKhuyenMaiRepository(CineTicketDbContext db) => _db = db;

        public Task<HoaDonKhuyenMai?> GetByHoaDonAsync(int maHd) =>
            _db.HoaDonKhuyenMais.AsNoTracking().FirstOrDefaultAsync(x => x.MaHd == maHd);

        public async Task AddAsync(HoaDonKhuyenMai entity)
        {
            _db.HoaDonKhuyenMais.Add(entity);
            await _db.SaveChangesAsync();
        }

        public Task<bool> ExistsUserUsedPromoAsync(int khuyenMaiId, string userId) =>
            _db.HoaDonKhuyenMais.AnyAsync(x => x.KhuyenMaiId == khuyenMaiId && x.UserId == userId);
    }
}
