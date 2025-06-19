using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface ILoaiPhimRepository
    {
        Task<List<LoaiPhim>> GetAllAsync();
        Task<LoaiPhim?> GetByIdAsync(int id);
        Task<LoaiPhim> CreateAsync(LoaiPhim loaiPhim);
        Task<bool> UpdateAsync(LoaiPhim loaiPhim);
        Task<bool> DeleteAsync(int id);
    }
}