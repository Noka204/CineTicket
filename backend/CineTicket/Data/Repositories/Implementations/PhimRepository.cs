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

        // Lấy toàn bộ phim + ChiTietLoaiPhim + LoaiPhim + SuatChieu
        public async Task<List<Phim>> GetAllAsync()
        {
            return await _context.Phims
                .Include(p => p.ChiTietLoaiPhims)
                    .ThenInclude(ct => ct.LoaiPhim)
                .Include(p => p.SuatChieus)
                .ToListAsync();
        }

        public async Task<Phim?> GetByIdAsync(int id, bool includeShowtimes = false, CancellationToken ct = default)
        {
            var q = _context.Phims
                .Include(p => p.ChiTietLoaiPhims)
                    .ThenInclude(ctp => ctp.LoaiPhim)
                .AsQueryable();

            if (includeShowtimes)
                q = q.Include(p => p.SuatChieus);

            return await q.FirstOrDefaultAsync(p => p.MaPhim == id, ct);
        }

        // Thêm phim mới
        public async Task<Phim> CreateAsync(Phim phim)
        {
            _context.Phims.Add(phim);
            await _context.SaveChangesAsync(); // Cần thiết để có MaPhim
            return phim;
        }

        // Cập nhật phim
        public async Task<bool> UpdateAsync(Phim phim)
        {
            _context.Phims.Update(phim);
            return await _context.SaveChangesAsync() > 0;
        }

        // Xoá phim và liên kết loại phim nếu cần
        public async Task<bool> DeleteAsync(int id)
        {
            var phim = await _context.Phims.FindAsync(id);
            if (phim == null) return false;

            _context.Phims.Remove(phim);
            return await _context.SaveChangesAsync() > 0;
        }

        // Truy vấn dạng IQueryable để dùng Include bên ngoài
        public IQueryable<Phim> Query()
        {
            return _context.Phims.AsQueryable();
        }

        // Thêm danh sách loại phim cho phim
        public async Task AddLoaiPhimToPhimAsync(List<ChiTietLoaiPhim> list)
        {
            await _context.ChiTietLoaiPhims.AddRangeAsync(list);
            await _context.SaveChangesAsync();
        }


        // Cập nhật loại phim (xóa hết và thêm lại)
        public async Task UpdateLoaiPhimOfPhimAsync(int maPhim, List<int> maLoaiPhims)
        {
            var old = _context.ChiTietLoaiPhims.Where(ct => ct.MaPhim == maPhim);
            _context.ChiTietLoaiPhims.RemoveRange(old);

            var newList = maLoaiPhims.Select(loaiId => new ChiTietLoaiPhim
            {
                MaPhim = maPhim,
                MaLoaiPhim = loaiId
            });

            await _context.ChiTietLoaiPhims.AddRangeAsync(newList);
            await _context.SaveChangesAsync();
        }
    }
}
