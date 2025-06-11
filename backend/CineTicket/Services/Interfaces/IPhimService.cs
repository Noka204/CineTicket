using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IPhimService
    {
        Task<IEnumerable<Phim>> GetAllAsync();
        Task<Phim?> GetByIdAsync(int id);
        Task<Phim> CreateAsync(Phim phim);
        Task<bool> UpdateAsync(Phim phim);
        Task<bool> DeleteAsync(int id);
    }
}
