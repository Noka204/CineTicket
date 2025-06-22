//using CineTicket.Models;
//using CineTicket.Data.Repositories.Interfaces;
//using CineTicket.Services.Interfaces;
//using CineTicket.DTOs.ChiTietHoaDon;
//using Microsoft.EntityFrameworkCore;

//namespace CineTicket.Services.Implementations
//{
//    public class HoaDonService : IHoaDonService
//    {
//        private readonly IHoaDonRepository _repo;
//        public HoaDonService(IHoaDonRepository repo) => _repo = repo;

//        public Task<IEnumerable<HoaDon>> GetAllAsync() => _repo.GetAllAsync();
//        public Task<HoaDon?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
//        public async Task<HoaDon> CreateWithDetailsAsync(HoaDon hoaDon, List<CreateChiTietHoaDonDTO>? details)
//        {
//            _context.HoaDons.Add(hoaDon);
//            await _context.SaveChangesAsync();

//            if (details != null)
//            {
//                foreach (var d in details)
//                {
//                    _context.ChiTietHoaDons.Add(new ChiTietHoaDon
//                    {
//                        MaHd = hoaDon.MaHd,
//                        MaVe = d.MaVe,
//                        MaBn = d.MaBn,
//                        SoLuong = d.SoLuong
//                    });
//                }
//                await _context.SaveChangesAsync();
//            }

//            return hoaDon;
//        }
//        public Task<bool> UpdateAsync(HoaDon model) => _repo.UpdateAsync(model);
//        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
//    }
//}
