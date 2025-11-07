using CineTicket.DTOs.Ve;
using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IVeService
    {
        Task<IEnumerable<Ve>> GetAllAsync();
        Task<Ve?> GetByIdAsync(int id);
        Task<Ve> CreateAsync(Ve ve);
        Task<bool> UpdateAsync(Ve ve);
        Task<bool> DeleteAsync(int id);

        Task<(bool success, string message, GiuGheResponse? data, int statusCode)>
            GiuGheAsync(GiuGheRequest request, string userId);

        Task<(bool success, string message)> BoGiuGheAsync(GiuGheRequest request, string userId);

        Task<Ve?> GetByGheAndSuatAsync(int maGhe, int maSuat);
        Task<TinhGiaVeResult> TinhGiaVeAsync(int maGhe, int maSuat);

        // MỚI: trả danh sách ghế do user hiện tại đang giữ trong một suất
        Task<IReadOnlyList<HeldSeatDto>> GetHeldByUserAsync(int maSuat, string userId);
    }
}
