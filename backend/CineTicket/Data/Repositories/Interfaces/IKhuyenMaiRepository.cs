using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IKhuyenMaiRepository
    {
        Task<List<KhuyenMai>> GetAllAsync();
        Task<KhuyenMai?> GetAsync(int id);
        Task AddAsync(KhuyenMai entity);
        Task UpdateAsync(KhuyenMai entity);
        Task<bool> DeleteAsync(int id);
    }
}
