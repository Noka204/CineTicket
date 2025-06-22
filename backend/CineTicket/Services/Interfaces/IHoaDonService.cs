using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IHoaDonService
    {
        Task<IEnumerable<HoaDon>> GetAllAsync();
        Task<HoaDon?> GetByIdAsync(int id);
        Task<HoaDon> CreateAsync(HoaDon model);
        Task<bool> UpdateAsync(HoaDon model);
        Task<bool> DeleteAsync(int id);
    }
}
