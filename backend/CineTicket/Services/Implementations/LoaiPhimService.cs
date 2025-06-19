using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class LoaiPhimService : ILoaiPhimService
    {
        private readonly ILoaiPhimRepository _repo;

        public LoaiPhimService(ILoaiPhimRepository repo)
        {
            _repo = repo;
        }

        public Task<List<LoaiPhim>> GetAllAsync() => _repo.GetAllAsync();
        public Task<LoaiPhim?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<LoaiPhim> CreateAsync(LoaiPhim loaiPhim) => _repo.CreateAsync(loaiPhim);
        public Task<bool> UpdateAsync(LoaiPhim loaiPhim) => _repo.UpdateAsync(loaiPhim);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
