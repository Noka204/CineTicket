using CineTicket.Models;

namespace CineTicket.Data.Repositories.Interfaces
{
    public interface ISuatChieuRepository
    {
        Task<IEnumerable<SuatChieu>> GetAllAsync();
        Task<SuatChieu?> GetByIdAsync(int id);

        Task<List<SuatChieu>> GetByPhimAsync(
            int maPhim,
            int? maRap = null,
            int? maPhong = null,
            DateOnly? ngay = null);

        Task<SuatChieu> CreateAsync(SuatChieu suatChieu);
        Task<bool> UpdateAsync(SuatChieu suatChieu);
        Task<bool> DeleteAsync(int id);
    }
}
