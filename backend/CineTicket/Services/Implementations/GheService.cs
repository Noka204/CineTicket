using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class GheService : IGheService
    {
        private readonly IGheRepository _gheRepo;

        public GheService(IGheRepository gheRepo)
        {
            _gheRepo = gheRepo;
        }

        public Task<IEnumerable<Ghe>> GetAllAsync() => _gheRepo.GetAllAsync();
        public Task<Ghe?> GetByIdAsync(int id) => _gheRepo.GetByIdAsync(id);
        public Task<Ghe> CreateAsync(Ghe ghe) => _gheRepo.CreateAsync(ghe);
        public Task<bool> UpdateAsync(Ghe ghe) => _gheRepo.UpdateAsync(ghe);
        public Task<bool> DeleteAsync(int id) => _gheRepo.DeleteAsync(id);
    }
}
