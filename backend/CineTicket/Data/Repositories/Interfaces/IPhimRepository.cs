using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IPhimRepository
    {
        Task<List<Phim>> GetAllAsync();
        Task<Phim?> GetByIdAsync(int id);
        Task<Phim> CreateAsync(Phim phim);
        Task<bool> UpdateAsync(Phim phim);
        Task<bool> DeleteAsync(int id);
        IQueryable<Phim> Query();
        Task AddLoaiPhimToPhimAsync(List<ChiTietLoaiPhim> list);
        Task UpdateLoaiPhimOfPhimAsync(int maPhim, List<int> maLoaiPhims);
    }
}
