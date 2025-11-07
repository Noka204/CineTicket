using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IPhimRepository
    {
        Task<List<Phim>> GetAllAsync();
        IQueryable<Phim> Query();
        Task<Phim?> GetByIdAsync(int id, bool includeShowtimes = false, CancellationToken ct = default);
        Task<Phim> CreateAsync(Phim phim);
        Task<bool> UpdateAsync(Phim phim);
        Task<bool> DeleteAsync(int id);

        Task AddLoaiPhimToPhimAsync(List<ChiTietLoaiPhim> list);
        Task UpdateLoaiPhimOfPhimAsync(int maPhim, List<int> maLoaiPhims);
    }
}
