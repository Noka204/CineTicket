using AutoMapper;
using CineTicket.Data.Repositories.Interfaces;
using CineTicket.DTOs;
using CineTicket.Models;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services.Implementations
{
    public class BapNuocService : IBapNuocService
    {
        private readonly IBapNuocRepository _repo;

        public BapNuocService(IBapNuocRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<BapNuoc>> GetAllAsync()
        {
            return _repo.GetAllAsync();
        }

        public Task<BapNuoc?> GetByIdAsync(int id)
        {
            return _repo.GetByIdAsync(id);
        }
        public Task<BapNuoc> CreateAsync(BapNuoc model)
        {
            return _repo.CreateAsync(model);
        }

        public Task<bool> UpdateAsync(BapNuoc model)
        {
            return _repo.UpdateAsync(model);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _repo.DeleteAsync(id);
        }
    }

}
