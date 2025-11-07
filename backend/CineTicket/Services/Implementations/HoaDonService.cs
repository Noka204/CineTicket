using AutoMapper;
using CineTicket.Data;
using CineTicket.DTOs.HoaDon;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CineTicket.Services.Implementations
{
    public class HoaDonService : IHoaDonService
    {
        private readonly IHoaDonRepository _repo;
        private readonly IMapper _mapper;
        private readonly CineTicketDbContext _db;
        private readonly IVeService _veService;
        private readonly ILogger<HoaDonService> _logger;

        public HoaDonService(
            IHoaDonRepository repo,
            IMapper mapper,
            CineTicketDbContext db,
            IVeService veService,
            ILogger<HoaDonService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _db = db;
            _veService = veService;
            _logger = logger;
        }

        public Task<HoaDon?> GetByIdAsync(int maHd) => _repo.GetByIdAsync(maHd);

        public async Task<IEnumerable<HoaDon>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<bool> UpdateAsync(UpdateHoaDonDTO dto)
        {
            var entity = _mapper.Map<HoaDon>(dto);
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        /// <summary>
        /// Helper: SaveChangesAsync có log
        /// </summary>
        private async Task<int> SaveChangesWithLogAsync(string contextInfo)
        {
            try
            {
                var rows = await _db.SaveChangesAsync();
                _logger.LogInformation("✅ SaveChanges OK ({Context}) — Rows affected: {Rows}", contextInfo, rows);
                return rows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SaveChanges FAIL ({Context})", contextInfo);
                if (ex.InnerException != null)
                {
                    _logger.LogError("👉 InnerException: {Inner}", ex.InnerException.Message);
                }
                throw;
            }
        }

        public async Task<HoaDon> CreateWithDetailsAsync(CreateHoaDonDTO dto, string userId)
        {
            var now = DateTime.Now;
            var graceCutoff = now.AddSeconds(-5);

            // ---- Gom tất cả MaVe từ SeatIds hoặc ChiTietHoaDons ----
            // 1) Ưu tiên MaVe từ chi tiết (nếu có)
            var veIdsFromDto = (dto.ChiTietHoaDons?
                                    .Where(x => x.MaVe.HasValue)
                                    .Select(x => x.MaVe!.Value)
                                    .Distinct()
                                    .ToList()) ?? new();

            // 2) Fallback theo MaGhe từ SeatIds (FE đang gửi MaGhe ở đây)
            var seatIdsFromDto = (dto.SeatIds?.Distinct().ToList()) ?? new();

            IQueryable<Ve> q = _db.Ves.AsQueryable().Where(v => v.MaSuat == dto.MaSuat);

            if (veIdsFromDto.Any())
            {
                q = q.Where(v => veIdsFromDto.Contains(v.MaVe));
            }
            else if (seatIdsFromDto.Any())
            {
                q = q.Where(v => v.MaGhe.HasValue && seatIdsFromDto.Contains(v.MaGhe.Value));
            }

            var vesRaw = await q.ToListAsync();


            var holdWindow = TimeSpan.FromMinutes(5);
            var validVes = vesRaw.Where(v =>
                v.TrangThai == "TamGiu" &&
                v.ThoiGianTamGiu.HasValue &&
                (v.ThoiGianTamGiu.Value + holdWindow) > now &&
                v.NguoiGiuId == userId
            ).ToList();


            // ---- Gom bắp nước ----
            var bapNuocList = dto.ChiTietHoaDons?
                .Where(x => x.MaBn.HasValue && x.SoLuong > 0)
                .ToList() ?? new();

            if (validVes.Count == 0 && bapNuocList.Count == 0)
                throw new InvalidOperationException("Không có vé/bắp hợp lệ để tạo hóa đơn.");

            // ---- Tạo hóa đơn ----
            var hd = new HoaDon
            {
                ApplicationUserId = userId,
                NgayLap = now,
                TrangThai = "Chưa thanh toán",
                HinhThucThanhToan = string.IsNullOrWhiteSpace(dto.HinhThucThanhToan) ? "Momo" : dto.HinhThucThanhToan,
                TongTien = 0m,
                MaSuat = dto.MaSuat
            };

            _db.HoaDons.Add(hd);
            await SaveChangesWithLogAsync("Insert HoaDon");

            // ---- Chi tiết hóa đơn + tính tiền ----
            var details = new List<ChiTietHoaDon>();
            decimal tongTien = 0m;

            // Vé
            foreach (var v in validVes)
            {
                var giaVeInfo = await _veService.TinhGiaVeAsync(v.MaGhe ?? 0, v.MaSuat ?? 0);
                if (giaVeInfo == null)
                    throw new InvalidOperationException($"Không tính được giá vé cho ghế {v.MaGhe}.");

                var donGia = giaVeInfo.GiaCuoiCung;

                details.Add(new ChiTietHoaDon
                {
                    MaHd = hd.MaHd,
                    MaVe = v.MaVe,
                    SoLuong = 1,
                    DonGia = donGia
                });

                tongTien += donGia;

                // cập nhật trạng thái vé
                v.TrangThai = "TamGiu";
            }

            // Bắp nước
            foreach (var li in bapNuocList)
            {
                var bn = await _db.BapNuocs
                    .FirstOrDefaultAsync(x => x.MaBn == li.MaBn!.Value)
                    ?? throw new InvalidOperationException("Bắp nước không hợp lệ.");

                var donGia = bn.Gia ?? 0m;

                details.Add(new ChiTietHoaDon
                {
                    MaHd = hd.MaHd,
                    MaBn = bn.MaBn,
                    SoLuong = li.SoLuong,
                    DonGia = donGia
                });

                tongTien += donGia * li.SoLuong;
            }

            if (details.Any())
            {
                _db.ChiTietHoaDons.AddRange(details);
                await SaveChangesWithLogAsync("Insert ChiTietHoaDon");
            }

            // ---- Update tổng tiền ----
            hd.TongTien = tongTien;
            _db.HoaDons.Update(hd);
            await SaveChangesWithLogAsync("Update TongTien HoaDon");

            return hd;
        }
        public async Task<(int total, List<MyPaidMovieItemDto> items)> GetMyPaidMoviesAsync(
    string userId, int skip, int take)
        {
            var baseQuery = _db.HoaDons
                .AsNoTracking()
                .Where(h => h.ApplicationUserId == userId
                         && (h.TrangThai == "Đã thanh toán" || h.TrangThai == "Paid" || h.TrangThai == "Da thanh toan"));

            var total = await baseQuery.CountAsync();

            var query =
                from h in baseQuery
                join s in _db.SuatChieus.AsNoTracking() on h.MaSuat equals s.MaSuat into sj
                from s in sj.DefaultIfEmpty()
                orderby h.NgayLap descending
                select new MyPaidMovieItemDto
                {
                    MaHd = h.MaHd,
                    NgayLap = h.NgayLap,
                    HinhThucThanhToan = h.HinhThucThanhToan,
                    TongTien = h.TongTien,

                    MaPhim = s != null && s.MaPhim.HasValue ? s.MaPhim.Value : 0,
                    TenPhim = s != null && s.MaPhimNavigation != null ? s.MaPhimNavigation.TenPhim : null,
                    Poster = s != null && s.MaPhimNavigation != null ? s.MaPhimNavigation.Poster : null,
                    ThoiGianBatDau = s != null ? s.ThoiGianBatDau : null,
                    GioChieu = s != null ? s.GioChieu : null
                };

            var items = await query
                .Skip(skip < 0 ? 0 : skip)
                .Take(take <= 0 ? 8 : (take > 50 ? 50 : take))
                .ToListAsync();

            return (total, items);
        }

    }

}
