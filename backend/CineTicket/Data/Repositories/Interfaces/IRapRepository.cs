using System.Collections.Generic;
using System.Threading.Tasks;
using CineTicket.Models;

namespace CineTicket.Repositories.Interfaces
{
    public interface IRapRepository
    {
        Task<List<Rap>> GetAllAsync();
        Task<Rap?> GetByIdAsync(int id);
        Task<Rap> AddAsync(Rap entity);
        Task<Rap?> UpdateAsync(Rap entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // extra
        Task<List<string>> GetCitiesAsync();
        Task<List<Rap>> GetByCityAsync(string thanhPho);
    }
}
