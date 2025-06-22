using CineTicket.Data.Repositories.Interfaces;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class SuatChieuService : ISuatChieuService
    {
        private readonly ISuatChieuRepository _suatRepo;

        public SuatChieuService(ISuatChieuRepository suatRepo)
        {
            _suatRepo = suatRepo;
        }

        public Task<IEnumerable<SuatChieu>> GetAllAsync() => _suatRepo.GetAllAsync();
        public Task<SuatChieu?> GetByIdAsync(int id) => _suatRepo.GetByIdAsync(id);
        public Task<List<SuatChieu>> GetByPhimIdAsync(int maPhim)
            => _suatRepo.GetByPhimIdAsync(maPhim);

        public Task<SuatChieu> CreateAsync(SuatChieu suatChieu) => _suatRepo.CreateAsync(suatChieu);
        public Task<bool> UpdateAsync(SuatChieu suatChieu) => _suatRepo.UpdateAsync(suatChieu);
        public Task<bool> DeleteAsync(int id) => _suatRepo.DeleteAsync(id);
    }
}
