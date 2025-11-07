using CineTicket.Data;
using CineTicket.DTOs.KhuyenMai;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class KhuyenMaiService : IKhuyenMaiService
    {
        private readonly IKhuyenMaiRepository _repo;
        private readonly CineTicketDbContext _db;

        public KhuyenMaiService(IKhuyenMaiRepository repo, CineTicketDbContext db)
        {
            _repo = repo; _db = db;
        }

        public Task<List<KhuyenMai>> GetAllAsync() => _repo.GetAllAsync();

        public Task<KhuyenMai?> GetAsync(int id) => _repo.GetAsync(id);

        public async Task<KhuyenMai> CreateAsync(KhuyenMaiCreateDto dto)
        {
            var km = new KhuyenMai
            {
                Ten = dto.Ten,
                IsActive = dto.IsActive,
                LoaiGiam = dto.LoaiGiam,
                MucGiam = dto.MucGiam,
                BatDau = dto.BatDau,
                KetThuc = dto.KetThuc
            };
            await _repo.AddAsync(km);
            return km;
        }

        public async Task<KhuyenMai?> UpdateAsync(KhuyenMaiUpdateDto dto)
        {
            var km = await _repo.GetAsync(dto.Id);
            if (km is null) return null;

            km.Ten = dto.Ten;
            km.IsActive = dto.IsActive;
            km.LoaiGiam = dto.LoaiGiam;
            km.MucGiam = dto.MucGiam;
            km.BatDau = dto.BatDau;
            km.KetThuc = dto.KetThuc;
            await _repo.UpdateAsync(km);
            return km;
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
