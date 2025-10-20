using CineTicket.Data.Repositories.Interfaces;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class SuatChieuService : ISuatChieuService
    {
        private readonly ISuatChieuRepository _repo;

        public SuatChieuService(ISuatChieuRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<SuatChieu>> GetAllAsync()
            => _repo.GetAllAsync();

        public Task<SuatChieu?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public async Task<List<SuatChieuDTO>> GetByPhimIdAsync(
            int maPhim,
            int? maRap = null,
            int? maPhong = null,
            DateOnly? ngay = null)
        {
            var list = await _repo.GetByPhimAsync(maPhim, maRap, maPhong, ngay);
            return list.Select(ToDto).ToList();
        }

        public Task<SuatChieu> CreateAsync(SuatChieu suatChieu)
            => _repo.CreateAsync(suatChieu);

        public Task<bool> UpdateAsync(SuatChieu suatChieu)
            => _repo.UpdateAsync(suatChieu);

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);

        // ===== Mapping =====
        private static SuatChieuDTO ToDto(SuatChieu s)
        {
            var rap = s.MaPhongNavigation?.Raps;
            return new SuatChieuDTO
            {
                MaSuat = s.MaSuat,
                MaPhim = s.MaPhim,
                TenPhim = s.MaPhimNavigation?.TenPhim,
                MaRap = rap?.MaRap,
                TenRap = rap?.TenRap,
                MaPhong = s.MaPhong,
                TenPhong = s.MaPhongNavigation?.TenPhong,
                ThoiGianBatDau = s.ThoiGianBatDau,
            };
        }
    }
}
