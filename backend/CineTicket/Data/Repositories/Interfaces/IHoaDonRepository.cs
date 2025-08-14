using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IHoaDonRepository
    {
        Task<HoaDon> CreateAsync(HoaDon hoaDon);
        Task<HoaDon?> FindByClientTokenAsync(string userId, string clientToken);
        Task<HoaDon?> GetByIdAsync(int maHd);
        Task<IEnumerable<HoaDon>> GetAllAsync();
        Task<bool> UpdateAsync(HoaDon hoaDon);
        Task<bool> DeleteAsync(int id);
    }
}