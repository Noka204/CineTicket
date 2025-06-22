using CineTicket.DTOs.ChiTietHoaDon;
using CineTicket.Models;

public interface IHoaDonService
{
    Task<IEnumerable<HoaDon>> GetAllAsync();
    Task<HoaDon?> GetByIdAsync(int id);
    Task<HoaDon> CreateWithDetailsAsync(HoaDon hoaDon, List<CreateChiTietHoaDonDTO>? details);
    Task<bool> UpdateWithDetailsAsync(int id, HoaDon hoaDon, List<CreateChiTietHoaDonDTO>? details);
    Task<bool> DeleteWithDetailsAsync(int id);
}
