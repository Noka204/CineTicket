using CineTicket.DTOs.Ghe;
using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IGheService
    {
        Task<IEnumerable<Ghe>> GetAllAsync();
        Task<Ghe?> GetByIdAsync(int id);
        Task<IEnumerable<GheTrangThaiDTO>> GetGheTrangThaiAsync(int maPhong,int maSuatChieu);
        Task<Ghe> CreateAsync(Ghe ghe);
        Task<bool> UpdateAsync(Ghe ghe);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Ghe>> GetByPhongAsync(int maPhong);
        //get trạng thái ghế theo phòng và suất chiếu
        Task<IEnumerable<object>> GetTrangThaiGheAsync(int maPhong, int maSuat);
    }
}
