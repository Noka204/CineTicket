namespace CineTicket.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<(DateTime Ngay, decimal TongTien)>> GetDoanhThuTheoNgayTrongThang(int year, int month);
    }
}
