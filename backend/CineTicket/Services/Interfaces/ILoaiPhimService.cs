using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface ILoaiPhimService
    {
        Task<List<LoaiPhim>> GetAllAsync();
        Task<LoaiPhim?> GetByIdAsync(int id);
        Task<LoaiPhim> CreateAsync(LoaiPhim loaiPhim);
        Task<bool> UpdateAsync(LoaiPhim loaiPhim);
        Task<bool> DeleteAsync(int id);
    }
}
