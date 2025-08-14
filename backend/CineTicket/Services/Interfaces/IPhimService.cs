using CineTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Interfaces
{
    public interface IPhimService
    {
        Task<IEnumerable<Phim>> GetAllAsync();
        Task<Phim?> GetByIdAsync(int id);
        Task UpdateIsHotStatusAsync(int phimId);
        Task<Phim> CreateAsync(Phim phim);
        Task<bool> UpdateAsync(Phim phim);
        Task<bool> DeleteAsync(int id);
        IQueryable<Phim> Query();
        Task AddLoaiPhimToPhimAsync(List<ChiTietLoaiPhim> chiTietList);
        Task UpdateLoaiPhimOfPhimAsync(int maPhim, List<int> maLoaiPhims);

    }
}
