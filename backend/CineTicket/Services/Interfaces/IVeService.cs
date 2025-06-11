using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IVeService
    {
        Task<IEnumerable<Ve>> GetAllAsync();
        Task<Ve?> GetByIdAsync(int id);
        Task<Ve> CreateAsync(Ve ve);
        Task<bool> UpdateAsync(Ve ve);
        Task<bool> DeleteAsync(int id);
    }
}
