using CineTicket.Models;

public interface IHoaDonRepository
{
    Task<IEnumerable<HoaDon>> GetAllAsync();
    Task<HoaDon?> GetByIdAsync(int id);
    Task<HoaDon> CreateAsync(HoaDon model);
    Task<bool> UpdateAsync(HoaDon model);
    Task<bool> DeleteWithDetailsAsync(int id);
}
