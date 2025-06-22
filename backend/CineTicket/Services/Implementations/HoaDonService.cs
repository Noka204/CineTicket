using CineTicket.Models;
using CineTicket.Data.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class HoaDonService : IHoaDonService
    {
        private readonly IHoaDonRepository _repo;
        public HoaDonService(IHoaDonRepository repo) => _repo = repo;

        public Task<IEnumerable<HoaDon>> GetAllAsync() => _repo.GetAllAsync();
        public Task<HoaDon?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<HoaDon> CreateAsync(HoaDon model) => _repo.CreateAsync(model);
        public Task<bool> UpdateAsync(HoaDon model) => _repo.UpdateAsync(model);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
