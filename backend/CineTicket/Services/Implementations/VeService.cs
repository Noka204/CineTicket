using CineTicket.Data;
using CineTicket.DTOs.Ve;
using CineTicket.Hubs;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class VeService : IVeService
    {
        private readonly IVeRepository _veRepo;
        private readonly CineTicketDbContext _context;
        private readonly IHubContext<SeatHub> _hub;

        public VeService(IVeRepository veRepository, CineTicketDbContext context, IHubContext<SeatHub> hub)
        {
            _veRepo = veRepository;
            _context = context;
            _hub = hub;

        }

        public Task<IEnumerable<Ve>> GetAllAsync() => _veRepo.GetAllAsync();
        public Task<Ve?> GetByIdAsync(int id) => _veRepo.GetByIdAsync(id);
        public Task<Ve> CreateAsync(Ve ve) => _veRepo.CreateAsync(ve);
        public Task<bool> UpdateAsync(Ve ve) => _veRepo.UpdateAsync(ve);
        public Task<bool> DeleteAsync(int id) => _veRepo.DeleteAsync(id);
        public async Task<(bool success, string message, GiuGheResponse? data, int statusCode)>GiuGheAsync(GiuGheRequest request, string? userId)
        {
            var now = DateTime.Now;

            var suat = await _context.SuatChieus.FirstOrDefaultAsync(s => s.MaSuat == request.MaSuat);
            if (suat == null) return (false, "Không tìm thấy suất chiếu.", null, 404);

            var ghe = await _context.Ghes.FirstOrDefaultAsync(g => g.MaGhe == request.MaGhe && g.MaPhong == suat.MaPhong);
            if (ghe == null) return (false, "Ghế không đúng phòng.", null, 404);
            var ve = await _veRepo.GetByGheAndSuatAsync(request.MaGhe, request.MaSuat);
            if (ve != null)
            {
                if (ve.TrangThai == "DaDat") return (false, "Ghế đã đặt.", null, 409);
                if (ve.TrangThai == "TamGiu" && ve.ThoiGianTamGiu > now) return (false, "Ghế đang giữ.", null, 409);

                ve.TrangThai = "TamGiu";
                ve.ThoiGianTamGiu = now.AddMinutes(3);
                ve.NguoiGiuId = userId; 
                await _veRepo.UpdateAsync(ve);

                var payload = new SeatUpdatePayload
                {
                    MaSuat = request.MaSuat,
                    MaGhe = request.MaGhe,
                    TrangThai = "TamGiu",
                    ThoiGianHetHan = ve.ThoiGianTamGiu?.ToString("HH:mm:ss"),
                    Reason = "hold",
                    HolderUserId = ve.NguoiGiuId
                };

                await _hub.Clients.Group(SeatHub.GroupName(request.MaSuat))
                                    .SendAsync("SeatUpdated", payload);

                return (true, "Đã giữ lại ghế.", new GiuGheResponse
                {
                    MaVe = ve.MaVe,
                    MaGhe = ve.MaGhe ?? 0,
                    MaSuat = ve.MaSuat ?? 0,
                    TrangThai = ve.TrangThai,
                    ThoiGianHetHan = payload.ThoiGianHetHan
                }, 200);
            }

            var newVe = new Ve
            {
                MaGhe = request.MaGhe,
                MaSuat = request.MaSuat,
                TrangThai = "TamGiu",
                ThoiGianTamGiu = now.AddMinutes(3),
                NguoiGiuId = userId 
            };
            await _veRepo.CreateAsync(newVe);

            var payloadNew = new SeatUpdatePayload
            {
                MaSuat = request.MaSuat,
                MaGhe = request.MaGhe,
                TrangThai = "TamGiu",
                ThoiGianHetHan = newVe.ThoiGianTamGiu?.ToString("HH:mm:ss"),
                Reason = "hold"
            };
            await _hub.Clients.Group(SeatHub.GroupName(request.MaSuat))
                                .SendAsync("SeatUpdated", payloadNew);

            return (true, "Đã giữ ghế mới thành công.", new GiuGheResponse
            {
                MaVe = newVe.MaVe,
                MaGhe = newVe.MaGhe ?? 0,
                MaSuat = newVe.MaSuat ?? 0,
                TrangThai = newVe.TrangThai,
                ThoiGianHetHan = payloadNew.ThoiGianHetHan
            }, 201);
        }

        public async Task<(bool success, string message)> BoGiuGheAsync(GiuGheRequest request, string? userId)
        {
            var ve = await _veRepo.GetByGheAndSuatAsync(request.MaGhe, request.MaSuat);
            if (ve == null) return (false, "Không tìm thấy vé");

            if (ve.TrangThai == "TamGiu")
            {
                ve.TrangThai = "Trong";
                ve.ThoiGianTamGiu = null;
                ve.NguoiGiuId = null;
                await _veRepo.UpdateAsync(ve);

                var payload = new SeatUpdatePayload
                {
                    MaSuat = request.MaSuat,
                    MaGhe = request.MaGhe,
                    TrangThai = "Trong",
                    Reason = "release"
                };
                await _hub.Clients.Group(SeatHub.GroupName(request.MaSuat))
                                    .SendAsync("SeatUpdated", payload);
            }

            return (true, "Đã trả ghế");
        }
        public async Task<TinhGiaVeResult> TinhGiaVeAsync(int maGhe, int maSuat)
        {
            var suat = await _context.SuatChieus
                .Include(s => s.MaPhongNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSuat == maSuat);

            if (suat == null) return null;

            var ghe = await _context.Ghes
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.MaGhe == maGhe &&
                    g.MaPhong == suat.MaPhong);

            if (ghe == null) return null;

            var ve = await _context.Ves
                .FirstOrDefaultAsync(v => v.MaGhe == maGhe && v.MaSuat == maSuat);

            string trangThai = "Trong";

            if (ve != null)
            {
                if (ve.TrangThai == "DaDat")
                {
                    trangThai = "DaDat";
                }
                else if (ve.TrangThai == "TamGiu")
                {
                    if (ve.ThoiGianTamGiu > DateTime.Now)
                    {
                        trangThai = "TamGiu";
                    }
                    else
                    {
                        ve.TrangThai = "Trong";
                        ve.ThoiGianTamGiu = null;
                        trangThai = "Trong";
                        await _context.SaveChangesAsync();
                    }
                }
            }

            decimal giaCoBan = 70000;
            decimal giaCuoiCung = giaCoBan;

            bool laVip = string.Equals(ghe.LoaiGhe, "Vip", StringComparison.OrdinalIgnoreCase);
            if (laVip) giaCuoiCung += 10000;

            bool laBanDem = false;
            if (TimeSpan.TryParse(suat.GioChieu, out var gio) && gio.Hours >= 18)
            {
                giaCuoiCung += 5000;
                laBanDem = true;
            }

            bool laCuoiTuan = false;
            if (suat.NgayChieu is { } ngay &&
                (ngay.DayOfWeek == DayOfWeek.Saturday || ngay.DayOfWeek == DayOfWeek.Sunday))
            {
                giaCuoiCung += 5000;
                laCuoiTuan = true;
            }

            return new TinhGiaVeResult
            {
                MaGhe = ghe.MaGhe,
                SoGhe = ghe.SoGhe,
                LoaiGhe = ghe.LoaiGhe,
                MaSuat = suat.MaSuat,
                GioChieu = suat.GioChieu,
                NgayChieu = suat.NgayChieu?.ToString("yyyy-MM-dd"),
                GiaCoBan = giaCoBan,
                GiaCuoiCung = giaCuoiCung,
                LaVip = laVip,
                LaBanDem = laBanDem,
                LaCuoiTuan = laCuoiTuan,
                TrangThai = trangThai
            };
        }
        public Task<Ve?> GetByGheAndSuatAsync(int maGhe, int maSuat)
        {
            return _veRepo.GetByGheAndSuatAsync(maGhe, maSuat);
        }
    }
}
