using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IPhongChieuRepository
    {
        Task<IEnumerable<PhongChieu>> GetAllAsync();
        Task<PhongChieu?> GetByIdAsync(int id);
        Task<PhongChieu> CreateAsync(PhongChieu phongChieu);
        Task<bool> UpdateAsync(PhongChieu phongChieu);
        Task<bool> DeleteAsync(int id);
    }
}
