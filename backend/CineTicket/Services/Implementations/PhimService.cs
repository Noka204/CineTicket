using CineTicket.Data;
using CineTicket.Models;
using CineTicket.Repositories.Implementations;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class PhimService : IPhimService
    {
        private readonly IPhimRepository _phimRepo;
        private readonly CineTicketDbContext _context;
        private readonly ISuatChieuService _suatChieuService;

        public PhimService(IPhimRepository phimRepo,CineTicketDbContext context,ISuatChieuService suatChieuService)
        {
            _phimRepo = phimRepo;
            _context = context;
            _suatChieuService = suatChieuService;
        }

        public async Task<IEnumerable<Phim>> GetAllAsync()
        {
            var phimList = await _phimRepo.GetAllAsync();
            return phimList.AsEnumerable();
        }

        public IQueryable<Phim> Query() => _phimRepo.Query();

        public Task<Phim?> GetByIdAsync(int id, bool includeShowtimes = false, CancellationToken ct = default)
            => _phimRepo.GetByIdAsync(id, includeShowtimes, ct);


        public async Task UpdateIsHotStatusAsync(int phimId)
        {
            var phim = await _phimRepo.GetByIdAsync(phimId);
            if (phim == null) return;

            var veBan = await _context.Ves
                .Include(v => v.MaSuatNavigation)
                .Where(v => v.MaSuatNavigation.MaPhim == phimId)
                .CountAsync();

            phim.IsHot = veBan >= 35 ? "1" : "0";
            await _phimRepo.UpdateAsync(phim);
        }
        public async Task AddLoaiPhimToPhimAsync(List<ChiTietLoaiPhim> list)
        {
            await _phimRepo.AddLoaiPhimToPhimAsync(list);
        }

        public async Task UpdateLoaiPhimOfPhimAsync(int maPhim, List<int> maLoaiPhims)
        {
            await _phimRepo.UpdateLoaiPhimOfPhimAsync(maPhim, maLoaiPhims);
        }
        public Task<Phim> CreateAsync(Phim phim) => _phimRepo.CreateAsync(phim);

        public Task<bool> UpdateAsync(Phim phim) => _phimRepo.UpdateAsync(phim);

        public Task<bool> DeleteAsync(int id) => _phimRepo.DeleteAsync(id);

    }
}
