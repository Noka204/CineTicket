using CineTicket.Data.Repositories.Interfaces;
using CineTicket.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Data.Repositories.Implementations
{
    public class BapNuocRepository : IBapNuocRepository
    {
        private readonly CineTicketDbContext _context;

        public BapNuocRepository(CineTicketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BapNuoc>> GetAllAsync()
        {
            return await _context.BapNuocs.ToListAsync();
        }

        public async Task<BapNuoc?> GetByIdAsync(int id)
        {
            return await _context.BapNuocs.FindAsync(id);
        }

        public async Task<BapNuoc> CreateAsync(BapNuoc model)
        {
            _context.BapNuocs.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> UpdateAsync(BapNuoc model)
        {
            var existing = await _context.BapNuocs.FindAsync(model.MaBn);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.BapNuocs.FindAsync(id);
            if (entity == null) return false;

            _context.BapNuocs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }


}
