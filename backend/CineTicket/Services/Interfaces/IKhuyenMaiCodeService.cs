using CineTicket.Models;
using CineTicket.DTOs.KhuyenMai;

namespace CineTicket.Services.Interfaces
{
    public interface IKhuyenMaiCodeService
    {
        Task<List<KhuyenMaiCode>> GetByPromotionAsync(int khuyenMaiId);
        Task<KhuyenMaiCode?> GetByIdAsync(int id);
        Task<KhuyenMaiCode> CreateAsync(KhuyenMaiCodeCreateDto dto);      // tạo 1 code hoặc bulk nếu dto.Count > 0
        Task<List<KhuyenMaiCode>> BulkGenerateAsync(int khuyenMaiId, int count, string? prefix, string? assignedToUserId);
        Task<bool> DeleteAsync(int id);
    }
}
