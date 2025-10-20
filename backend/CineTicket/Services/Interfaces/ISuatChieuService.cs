using CineTicket.DTOs;
using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface ISuatChieuService
    {
        Task<IEnumerable<SuatChieu>> GetAllAsync();
        Task<SuatChieu?> GetByIdAsync(int id);
        Task<List<SuatChieuDTO>> GetByPhimIdAsync(
            int maPhim,
            int? maRap = null,
            int? maPhong = null,
            DateOnly? ngay = null);
        Task<SuatChieu> CreateAsync(SuatChieu suatChieu);
        Task<bool> UpdateAsync(SuatChieu suatChieu);
        Task<bool> DeleteAsync(int id);
    }
}
