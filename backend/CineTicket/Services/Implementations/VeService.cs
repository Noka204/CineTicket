using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class VeService : IVeService
    {
        private readonly IVeRepository _veRepo;

        public VeService(IVeRepository veRepo)
        {
            _veRepo = veRepo;
        }

        public Task<IEnumerable<Ve>> GetAllAsync() => _veRepo.GetAllAsync();
        public Task<Ve?> GetByIdAsync(int id) => _veRepo.GetByIdAsync(id);
        public Task<Ve> CreateAsync(Ve ve) => _veRepo.CreateAsync(ve);
        public Task<bool> UpdateAsync(Ve ve) => _veRepo.UpdateAsync(ve);
        public Task<bool> DeleteAsync(int id) => _veRepo.DeleteAsync(id);
    }
}
