namespace CineTicket.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<List<(DateTime Ngay, decimal TongTien)>> GetDoanhThuTheoNgayTrongThang(int year, int month);
    }
}
