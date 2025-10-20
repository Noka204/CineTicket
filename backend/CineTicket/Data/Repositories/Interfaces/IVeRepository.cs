using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IVeRepository
    {
        Task<IEnumerable<Ve>> GetAllAsync();
        Task<Ve?> GetByIdAsync(int id);
        Task<Ve> CreateAsync(Ve model);
        Task<bool> UpdateAsync(Ve model);
        Task<bool> DeleteAsync(int id);

        Task<Ve?> GetByGheAndSuatAsync(int maGhe, int maSuat);

        // NEW
        Task<IEnumerable<Ve>> GetHoldsByUserAsync(string userId, int? maSuat = null);
    }
}
