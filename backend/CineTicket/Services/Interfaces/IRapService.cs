using System.Collections.Generic;
using System.Threading.Tasks;
using CineTicket.DTOs.Rap;

namespace CineTicket.Services.Interfaces
{
    public interface IRapService
    {
        Task<List<RapDTO>> GetAllAsync();
        Task<RapDTO?> GetByIdAsync(int id);
        Task<RapDTO> CreateAsync(RapCreateDTO dto);
        Task<RapDTO?> UpdateAsync(int id, RapUpdateDTO dto);
        Task<bool> DeleteAsync(int id);

        Task<List<string>> GetCitiesAsync();
        Task<List<RapDTO>> GetByCityAsync(string thanhPho);
    }
}
