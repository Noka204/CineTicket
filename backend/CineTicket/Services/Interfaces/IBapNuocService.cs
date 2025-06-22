using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IBapNuocService
    {
        Task<IEnumerable<BapNuoc>> GetAllAsync();
        Task<BapNuoc?> GetByIdAsync(int id);
        Task<BapNuoc> CreateAsync(BapNuoc model);
        Task<bool> UpdateAsync(BapNuoc model);
        Task<bool> DeleteAsync(int id);
    }
}
