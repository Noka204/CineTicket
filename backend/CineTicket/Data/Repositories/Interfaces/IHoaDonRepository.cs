using CineTicket.Models;

namespace CineTicket.Data.Repositories.Interfaces
{
    public interface IHoaDonRepository
    {
        Task<IEnumerable<HoaDon>> GetAllAsync();
        Task<HoaDon?> GetByIdAsync(int id);
        Task<HoaDon> CreateAsync(HoaDon model);
        Task<bool> UpdateAsync(HoaDon model);
        Task<bool> DeleteAsync(int id);
    }
}
