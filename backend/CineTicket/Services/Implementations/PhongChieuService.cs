using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class PhongChieuService : IPhongChieuService
    {
        private readonly IPhongChieuRepository _phongRepo;

        public PhongChieuService(IPhongChieuRepository phongRepo)
        {
            _phongRepo = phongRepo;
        }

        public Task<IEnumerable<PhongChieu>> GetAllAsync() => _phongRepo.GetAllAsync();
        public Task<PhongChieu?> GetByIdAsync(int id) => _phongRepo.GetByIdAsync(id);
        public Task<PhongChieu> CreateAsync(PhongChieu phongChieu) => _phongRepo.CreateAsync(phongChieu);
        public Task<bool> UpdateAsync(PhongChieu phongChieu) => _phongRepo.UpdateAsync(phongChieu);
        public Task<bool> DeleteAsync(int id) => _phongRepo.DeleteAsync(id);
    }
}
