﻿using CineTicket.Models;

namespace CineTicket.Services.Interfaces
{
    public interface IPhongChieuService
    {
        Task<IEnumerable<PhongChieu>> GetAllAsync();
        Task<PhongChieu?> GetByIdAsync(int id);
        Task<PhongChieu> CreateAsync(PhongChieu phongChieu);
        Task<bool> UpdateAsync(PhongChieu phongChieu);
        Task<bool> DeleteAsync(int id);
    }
}
