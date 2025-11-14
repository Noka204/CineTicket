using CineTicket.DTOs.KhuyenMai;

namespace CineTicket.Services.Interfaces
{
    public interface IKhuyenMaiService
    {
        Task<List<KhuyenMaiOutDto>> GetAllAsync();
        Task<KhuyenMaiOutDto?> GetAsync(int id);
        Task<KhuyenMaiOutDto> CreateAsync(KhuyenMaiCreateDto dto);
        Task<KhuyenMaiOutDto?> UpdateAsync(KhuyenMaiUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        // NEW: validate mã (ưu tiên KhuyenMaiCode, fallback KhuyenMai.Ten)
        Task<ApplyCouponResult> ValidateAsync(string code, decimal? amount = null, string? userId = null);
    }
}
