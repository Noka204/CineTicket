using CineTicket.Data;
using CineTicket.DTOs.Ghe;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class GheService : IGheService
    {
        private readonly IGheRepository _gheRepo;
        private readonly CineTicketDbContext _context;


        public GheService(IGheRepository gheRepo,CineTicketDbContext context)
        {
            _gheRepo = gheRepo;
            _context = context;
        }

        public async Task<IEnumerable<Ghe>> GetAllAsync()
        {
            return await _context.Ghes
                .Include(g => g.MaPhongNavigation)  // cần include để map TenPhong
                .Include(g => g.Ves)                // cần include để map IsDisabled
                .ToListAsync();
        }

        public Task<Ghe?> GetByIdAsync(int id) => _gheRepo.GetByIdAsync(id);
        public async Task<IEnumerable<GheTrangThaiDTO>> GetGheTrangThaiAsync(int maPhong,int maSuatChieu)
        {
            return await _gheRepo.GetGheTrangThaiAsync(maPhong,maSuatChieu);
        }
        public async Task<IEnumerable<Ghe>> GetByPhongAsync(int maPhong)
        {
            return await _gheRepo.GetByPhongAsync(maPhong); // ✅
        }
        public Task<Ghe> CreateAsync(Ghe ghe) => _gheRepo.CreateAsync(ghe);
        public Task<bool> UpdateAsync(Ghe ghe) => _gheRepo.UpdateAsync(ghe);
        public Task<bool> DeleteAsync(int id) => _gheRepo.DeleteAsync(id);

        public async Task<IEnumerable<object>> GetTrangThaiGheAsync(int maPhong, int maSuat)
        {
            var now = DateTime.Now;

            var ghes = await _context.Ghes
                .Where(g => g.MaPhong == maPhong)
                .Select(g => new { g.MaGhe, g.SoGhe })
                .ToListAsync();

            var ves = await _context.Ves
                .Where(v => v.MaSuat == maSuat && v.MaGhe != null)
                .Select(v => new {
                    v.MaGhe,
                    v.TrangThai,
                    v.ThoiGianTamGiu
                })
                .ToListAsync();

            var veByGhe = ves.ToDictionary(v => v.MaGhe!.Value, v => v);

            var result = ghes.Select(g =>
            {
                string trangThai = "Trong";
                if (veByGhe.TryGetValue(g.MaGhe, out var ve))
                {
                    if (ve.TrangThai == "DaDat")
                        trangThai = "DaDat";
                    else if (ve.TrangThai == "TamGiu" && ve.ThoiGianTamGiu.HasValue && ve.ThoiGianTamGiu.Value > now)
                        trangThai = "TamGiu";
                }

                return new
                {
                    maGhe = g.MaGhe,
                    soGhe = g.SoGhe,
                    trangThai
                };
            });

            return result;
        }

    }
}
