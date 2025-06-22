using CineTicket.Models;

namespace CineTicket.Data.Repositories.Interfaces
{
    public interface IBapNuocRepository
    {
        Task<IEnumerable<BapNuoc>> GetAllAsync();
        Task<BapNuoc?> GetByIdAsync(int id);
        Task<BapNuoc> CreateAsync(BapNuoc model);
        Task<bool> UpdateAsync(BapNuoc model);
        Task<bool> DeleteAsync(int id);
    }
}
