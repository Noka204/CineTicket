using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<(DateTime Ngay, decimal TongTien)>> GetDoanhThuTheoNgayTrongThang(int year, int month)
        {
            return await _repository.GetDoanhThuTheoNgayTrongThang(year, month);
        }
    }
}
