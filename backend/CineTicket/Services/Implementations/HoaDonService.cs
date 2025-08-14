using AutoMapper;
using CineTicket.Data;
using CineTicket.DTOs.HoaDon;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class HoaDonService : IHoaDonService
    {
        private readonly IHoaDonRepository _repo;
        private readonly IMapper _mapper;
        private readonly CineTicketDbContext _db;

        public HoaDonService(IHoaDonRepository repo, IMapper mapper,CineTicketDbContext db)
        {
            _repo = repo;
            _mapper = mapper;
            _db = db;
        }

        public Task<HoaDon?> GetByIdAsync(int maHd) => _repo.GetByIdAsync(maHd);

        public async Task<HoaDon> CreateWithDetailsAsync(CreateHoaDonDTO dto, string userId)
        {
            // 0) Idempotency: trả lại hóa đơn cũ nếu đã tạo với cùng ClientToken
            if (!string.IsNullOrWhiteSpace(dto.ClientToken))
            {
                var existed = await _repo.FindByClientTokenAsync(userId, dto.ClientToken);
                if (existed != null) return existed;
            }

            using var tx = await _db.Database.BeginTransactionAsync();

            // 1) Gom list chi tiết vé từ SeatIds hoặc từ dto.ChiTietHoaDons
            var wantVeIds = new List<int>();   // danh sách MaVe (nếu FE gửi MaVe)
            var wantGheIds = dto.SeatIds?.Distinct().ToList() ?? new();

            if (dto.ChiTietHoaDons != null && dto.ChiTietHoaDons.Count > 0)
            {
                // Ưu tiên MaVe nếu có
                wantVeIds.AddRange(dto.ChiTietHoaDons.Where(x => x.MaVe.HasValue)
                                                     .Select(x => x.MaVe!.Value));
                // Có thể FE gửi MaGhe qua SeatIds: giữ nguyên wantGheIds
            }

            // 2) Lấy vé đang TamGiu theo MaVe hoặc theo (MaSuat + MaGhe)
            var ves = new List<Ve>();

            if (wantVeIds.Count > 0)
            {
                ves = await _db.Ves.Where(v => wantVeIds.Contains(v.MaVe)).ToListAsync();
            }
            else if (wantGheIds.Count > 0)
            {
                ves = await _db.Ves.Where(v => v.MaSuat == dto.MaSuat && wantGheIds.Contains(v.MaGhe ?? 0))
                                   .ToListAsync();
            }

            // Filter: chỉ nhận vé đang TamGiu còn hạn (tuỳ chính sách thêm check v.NguoiGiuId == userId)
            var now = DateTime.Now;
            ves = ves.Where(v => v.TrangThai == "TamGiu" && v.ThoiGianTamGiu.HasValue && v.ThoiGianTamGiu > now).ToList();

            if (ves.Count == 0 && dto.BapNuocId == null)
                throw new InvalidOperationException("Không có vé đang giữ hợp lệ để tạo hóa đơn.");

            // 3) Nếu có bắp nước
            BapNuoc? bn = null;
            var soLuongBn = 0;
            if (dto.BapNuocId.HasValue)
            {
                bn = await _db.BapNuocs.FirstOrDefaultAsync(x => x.MaBn == dto.BapNuocId.Value)
                     ?? throw new InvalidOperationException("Bắp nước không hợp lệ.");
                soLuongBn = Math.Max(dto.SoLuongBapNuoc, 1);
            }

            // 4) Tính tiền (đơn giá lấy từ DB)
            decimal tienGhe = ves.Sum(v => v.GiaVe ?? 0);
            decimal tienBap = (bn?.Gia ?? 0) * soLuongBn;
            decimal tongTien = tienGhe + tienBap;

            // 5) Tạo Hóa đơn + Chi tiết
            var hd = new HoaDon
            {
                ApplicationUserId = userId,
                NgayLap = DateTime.Now,
                TrangThai = "Chưa thanh toán",
                HinhThucThanhToan = string.IsNullOrWhiteSpace(dto.HinhThucThanhToan) ? "Momo" : dto.HinhThucThanhToan,
                TongTien = tongTien,
                ClientToken = dto.ClientToken,
                MaSuat = dto.MaSuat,                    // gợi ý thêm cột này trên HoaDon
                ChiTietHoaDons = new List<ChiTietHoaDon>()
            };

            foreach (var v in ves)
            {
                hd.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    MaVe = v.MaVe,
                    SoLuong = 1,
                    DonGia = v.GiaVe,                   // chốt đơn giá tại thời điểm tạo HĐ
                    MaVeNavigation = null               // đảm bảo không attach dư
                });
            }

            if (bn != null)
            {
                hd.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    MaBn = bn.MaBn,
                    SoLuong = soLuongBn,
                    DonGia = bn.Gia
                });
            }

            // 6) Lưu (một lần)
            _db.HoaDons.Add(hd);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return hd;
        }

        public async Task<IEnumerable<HoaDon>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<bool> UpdateAsync(UpdateHoaDonDTO dto)
        {
            var entity = _mapper.Map<HoaDon>(dto);
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }

}