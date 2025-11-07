using CineTicket.Models;
using CineTicket.DTOs.KhuyenMai;

namespace CineTicket.Services.Interfaces
{
    public interface IKhuyenMaiService
    {
        Task<List<KhuyenMai>> GetAllAsync();
        Task<KhuyenMai?> GetAsync(int id);
        Task<KhuyenMai> CreateAsync(KhuyenMaiCreateDto dto);
        Task<KhuyenMai?> UpdateAsync(KhuyenMaiUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
