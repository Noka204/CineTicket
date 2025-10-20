using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CineTicket.DTOs.Rap;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;

namespace CineTicket.Services
{
    public class RapService : IRapService
    {
        private readonly IRapRepository _repo;
        public RapService(IRapRepository repo) => _repo = repo;

        public async Task<List<RapDTO>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(x => new RapDTO
            {
                MaRap = x.MaRap,
                TenRap = x.TenRap,
                DiaChi = x.DiaChi,
                ThanhPho = x.ThanhPho,
                HoatDong = x.HoatDong
            }).ToList();
        }

        public async Task<RapDTO?> GetByIdAsync(int id)
        {
            var x = await _repo.GetByIdAsync(id);
            if (x is null) return null;
            return new RapDTO
            {
                MaRap = x.MaRap,
                TenRap = x.TenRap,
                DiaChi = x.DiaChi,
                ThanhPho = x.ThanhPho,
                HoatDong = x.HoatDong
            };
        }

        public async Task<RapDTO> CreateAsync(RapCreateDTO dto)
        {
            var entity = new Rap
            {
                TenRap = dto.TenRap,
                DiaChi = dto.DiaChi,
                ThanhPho = dto.ThanhPho,
                HoatDong = dto.HoatDong
            };
            var saved = await _repo.AddAsync(entity);
            return new RapDTO
            {
                MaRap = saved.MaRap,
                TenRap = saved.TenRap,
                DiaChi = saved.DiaChi,
                ThanhPho = saved.ThanhPho,
                HoatDong = saved.HoatDong
            };
        }

        public async Task<RapDTO?> UpdateAsync(int id, RapUpdateDTO dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing is null) return null;

            existing.TenRap = dto.TenRap;
            existing.DiaChi = dto.DiaChi;
            existing.ThanhPho = dto.ThanhPho;
            existing.HoatDong = dto.HoatDong;

            var saved = await _repo.UpdateAsync(existing);
            if (saved is null) return null;

            return new RapDTO
            {
                MaRap = saved.MaRap,
                TenRap = saved.TenRap,
                DiaChi = saved.DiaChi,
                ThanhPho = saved.ThanhPho,
                HoatDong = saved.HoatDong
            };
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public Task<List<string>> GetCitiesAsync() => _repo.GetCitiesAsync();

        public async Task<List<RapDTO>> GetByCityAsync(string thanhPho)
        {
            var list = await _repo.GetByCityAsync(thanhPho);
            return list.Select(x => new RapDTO
            {
                MaRap = x.MaRap,
                TenRap = x.TenRap,
                DiaChi = x.DiaChi,
                ThanhPho = x.ThanhPho,
                HoatDong = x.HoatDong
            }).ToList();
        }
    }
}
