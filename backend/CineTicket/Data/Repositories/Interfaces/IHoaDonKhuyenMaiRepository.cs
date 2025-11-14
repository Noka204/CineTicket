using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IHoaDonKhuyenMaiRepository
    {
        Task<HoaDonKhuyenMai?> GetByHoaDonAsync(int maHd);
        Task AddAsync(HoaDonKhuyenMai entity);
        Task<bool> ExistsUserUsedPromoAsync(int khuyenMaiId, string userId);
    }
}
