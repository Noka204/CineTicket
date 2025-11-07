using CineTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Interfaces
{
    public interface IPhimService
    {
        Task<IEnumerable<Phim>> GetAllAsync();
        IQueryable<Phim> Query();
        Task<Phim?> GetByIdAsync(int id, bool includeShowtimes = false, CancellationToken ct = default);
        Task UpdateIsHotStatusAsync(int phimId);
        Task<Phim> CreateAsync(Phim phim);
        Task<bool> UpdateAsync(Phim phim);
        Task<bool> DeleteAsync(int id);
        Task AddLoaiPhimToPhimAsync(List<ChiTietLoaiPhim> chiTietList);
        Task UpdateLoaiPhimOfPhimAsync(int maPhim, List<int> maLoaiPhims);

    }
}
