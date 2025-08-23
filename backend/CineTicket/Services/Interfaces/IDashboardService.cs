using CineTicket.DTOs.HoaDon.CineTicket.DTOs.Dashboard;

namespace CineTicket.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<DailyRevenueDTO>> GetDoanhThu30NgayGanNhatAsync(bool chiTinhDaThanhToan = true);
    }
}
