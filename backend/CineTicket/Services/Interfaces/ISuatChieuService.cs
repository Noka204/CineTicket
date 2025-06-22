using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface ISuatChieuService
    {
        Task<IEnumerable<SuatChieu>> GetAllAsync();
        Task<SuatChieu?> GetByIdAsync(int id);
        Task<List<SuatChieu>> GetByPhimIdAsync(int maPhim);


        Task<SuatChieu> CreateAsync(SuatChieu suatChieu);
        Task<bool> UpdateAsync(SuatChieu suatChieu);
        Task<bool> DeleteAsync(int id);
    }
}
