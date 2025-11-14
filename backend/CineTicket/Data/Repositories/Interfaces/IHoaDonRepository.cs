// ==============================
// File: Repositories/Interfaces/IHoaDonRepository.cs
// ==============================
using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IHoaDonRepository
    {
        Task<HoaDon> CreateAsync(HoaDon hoaDon);
        Task<HoaDon?> GetByIdAsync(int maHd);
        Task<IEnumerable<HoaDon>> GetAllAsync();
        Task<bool> UpdateAsync(HoaDon hoaDon);
        Task<bool> DeleteAsync(int id);

        // Để idempotency (nếu dùng)
        Task<HoaDon?> FindByClientTokenAsync(string userId, string clientToken);
    }
}
