using CineTicket.DTOs;
using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IBapNuocService
    {
        Task<IEnumerable<BapNuocDTO>> GetAllAsync(string targetLang = "vi", CancellationToken ct = default);
        Task<BapNuoc?> GetByIdAsync(int id);
        Task<BapNuoc> CreateAsync(BapNuoc model);
        Task<bool> UpdateAsync(BapNuoc model);
        Task<bool> DeleteAsync(int id);
    }
}
