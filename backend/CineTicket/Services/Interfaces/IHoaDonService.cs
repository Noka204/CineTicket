using CineTicket.DTOs.HoaDon;
using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IHoaDonService
    {
        Task<HoaDon> CreateWithDetailsAsync(CreateHoaDonDTO dto, string userId);
        Task<HoaDon?> GetByIdAsync(int id);
        Task<IEnumerable<HoaDon>> GetAllAsync();
        Task<bool> UpdateAsync(UpdateHoaDonDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}