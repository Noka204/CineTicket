using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IGheService
    {
        Task<IEnumerable<Ghe>> GetAllAsync();
        Task<Ghe?> GetByIdAsync(int id);
        Task<IEnumerable<Ghe>> GetByPhongAsync(int maPhong);
        Task<Ghe> CreateAsync(Ghe ghe);
        Task<bool> UpdateAsync(Ghe ghe);
        Task<bool> DeleteAsync(int id);
    }
}
