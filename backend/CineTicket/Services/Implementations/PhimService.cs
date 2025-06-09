using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class PhimService : IPhimService
    {
        private readonly IPhimRepository _phimRepo;

        public PhimService(IPhimRepository phimRepo)
        {
            _phimRepo = phimRepo;
        }

        public async Task<IEnumerable<Phim>> GetAllAsync()
        {
            var phimList = await _phimRepo.GetAllAsync();
            return phimList.AsEnumerable();
        }

        public Task<Phim?> GetByIdAsync(int id) => _phimRepo.GetByIdAsync(id);

        public Task<Phim> CreateAsync(Phim phim) => _phimRepo.CreateAsync(phim);

        public Task<bool> UpdateAsync(Phim phim) => _phimRepo.UpdateAsync(phim);

        public Task<bool> DeleteAsync(int id) => _phimRepo.DeleteAsync(id);
    }
}
