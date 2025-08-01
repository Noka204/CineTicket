﻿using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IGheRepository
    {
        Task<IEnumerable<Ghe>> GetAllAsync();
        Task<Ghe?> GetByIdAsync(int id);
        Task<Ghe> CreateAsync(Ghe ghe);
        Task<IEnumerable<Ghe>> GetByPhongAsync(int maPhong);

        Task<bool> UpdateAsync(Ghe ghe);
        Task<bool> DeleteAsync(int id);
    }
}
