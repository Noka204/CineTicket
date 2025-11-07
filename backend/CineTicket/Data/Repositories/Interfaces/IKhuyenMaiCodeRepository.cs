using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IKhuyenMaiCodeRepository
    {
        Task<KhuyenMaiCode?> GetByIdAsync(int id);
        Task<KhuyenMaiCode?> GetByCodeAsync(string code);
        Task<List<KhuyenMaiCode>> GetByPromotionAsync(int khuyenMaiId);
        Task AddAsync(KhuyenMaiCode code);
        Task AddBulkAsync(IEnumerable<KhuyenMaiCode> codes);
        Task<bool> DeleteAsync(int id);
    }
}
