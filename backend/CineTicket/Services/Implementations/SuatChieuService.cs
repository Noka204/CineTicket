using CineTicket.Data.Repositories.Interfaces;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Interfaces;
using System.Globalization;
namespace CineTicket.Services.Implementations
{
    public class SuatChieuService : ISuatChieuService
    {
        private readonly ISuatChieuRepository _repo;

        public SuatChieuService(ISuatChieuRepository repo) => _repo = repo;

        // CRUD
        public Task<IEnumerable<SuatChieu>> GetAllAsync() => _repo.GetAllAsync();
        public Task<SuatChieu?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<SuatChieu> CreateAsync(SuatChieu suatChieu) => _repo.CreateAsync(suatChieu);
        public Task<bool> UpdateAsync(SuatChieu suatChieu) => _repo.UpdateAsync(suatChieu);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        // ✅ Chỉ 1 hàm: lọc trực tiếp + map DTO, không dùng helper
        public async Task<List<SuatChieuDTO>> GetByPhimIdAsync(
    int maPhim, int? maRap = null, int? maPhong = null, DateOnly? ngay = null)
        {
            var all = await _repo.GetAllAsync();
            var q = all.Where(s => s.MaPhim == maPhim);

            if (maRap.HasValue)
            {
                q = q.Where(s =>
                    (s.MaRap.HasValue && s.MaRap.Value == maRap.Value) ||
                    (s.MaPhongNavigation?.Rap?.MaRap == maRap.Value));
            }

            if (maPhong.HasValue)
                q = q.Where(s => s.MaPhong == maPhong.Value);

            if (ngay.HasValue)
            {
                q = q.Where(s =>
                    (s.NgayChieu.HasValue && s.NgayChieu.Value == ngay.Value)
                    || (!s.NgayChieu.HasValue
                        && s.ThoiGianBatDau.HasValue
                        && DateOnly.FromDateTime(s.ThoiGianBatDau.Value) == ngay.Value));
            }

            // 🔎 Ẩn suất đã qua khi lọc đúng NGÀY HÔM NAY
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            if (ngay.HasValue && ngay.Value == today)
            {
                var nowT = now.TimeOfDay;

                q = q.Where(s =>
                    // Có ThoiGianBatDau → so sánh trực tiếp theo TimeOfDay
                    (s.ThoiGianBatDau.HasValue && s.ThoiGianBatDau.Value.TimeOfDay >= nowT)
                    // Không có → fallback parse GioChieu "HH:mm"
                    || (!s.ThoiGianBatDau.HasValue && ParseHHmm(s.GioChieu) >= nowT)
                );
            }

            var result = q
                .Select(s => new SuatChieuDTO
                {
                    MaSuat = s.MaSuat,
                    MaPhim = s.MaPhim,
                    TenPhim = s.MaPhimNavigation?.TenPhim,

                    MaRap = s.MaRap ?? s.MaPhongNavigation?.Rap?.MaRap,
                    TenRap = s.Rap?.TenRap ?? s.MaPhongNavigation?.Rap?.TenRap,

                    MaPhong = s.MaPhong,
                    TenPhong = s.MaPhongNavigation?.TenPhong,

                    ThoiGianBatDau = s.ThoiGianBatDau,
                    ThoiGianKetThuc = s.ThoiGianKetThuc,

                    NgayChieu = s.NgayChieu,
                    GioChieu = s.GioChieu
                })
                // Sắp xếp theo giờ chiếu tăng dần
                .OrderBy(s =>
                {
                    if (s.ThoiGianBatDau.HasValue) return s.ThoiGianBatDau.Value;
                    var d = s.NgayChieu ?? (ngay ?? today);
                    return d.ToDateTime(TimeOnly.FromTimeSpan(ParseHHmm(s.GioChieu)));
                })
                .ToList();

            return result;

            // ===== Helpers (local functions) =====
            static TimeSpan ParseHHmm(string? hhmm)
            {
                if (!string.IsNullOrWhiteSpace(hhmm) && TimeSpan.TryParse(hhmm, out var ts))
                    return ts;
                // fallback: "HH:mm" basic
                try
                {
                    var parts = (hhmm ?? "").Split(':');
                    var h = int.Parse(parts[0]); var m = int.Parse(parts[1]);
                    return new TimeSpan(h, m, 0);
                }
                catch { return TimeSpan.Zero; }
            }
        }

    }
}
