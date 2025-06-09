using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IVeRepository
    {
        Task<IEnumerable<Ve>> GetAllAsync();
        Task<Ve?> GetByIdAsync(int id);
        Task<Ve> CreateAsync(Ve ve);
        Task<bool> UpdateAsync(Ve ve);
        Task<bool> DeleteAsync(int id);
    }
}
