// SuatChieuRepository.cs
using CineTicket.Data.Repositories.Interfaces;
using CineTicket.Data;
using CineTicket.Models;
using Microsoft.EntityFrameworkCore;

public class SuatChieuRepository : ISuatChieuRepository
{
    private readonly CineTicketDbContext _context;
    public SuatChieuRepository(CineTicketDbContext context) => _context = context;

    public async Task<IEnumerable<SuatChieu>> GetAllAsync()
    {
        return await _context.SuatChieus
            .AsNoTracking()
            .Include(s => s.MaPhimNavigation)
            .Include(s => s.MaPhongNavigation)
                .ThenInclude(p => p!.Rap)
            .Include(s => s.Rap)             // nếu đã có FK trực tiếp MaRap -> Rap
            .Include(s => s.Ves)            // nếu cần biết trạng thái ghế/vé
            .ToListAsync();
    }

    public async Task<SuatChieu?> GetByIdAsync(int id)
    {
        return await _context.SuatChieus
            .AsNoTracking()
            .Include(s => s.MaPhimNavigation)
            .Include(s => s.MaPhongNavigation)
                .ThenInclude(p => p!.Rap)
            .Include(s => s.Rap)
            .FirstOrDefaultAsync(s => s.MaSuat == id);
    }

    public async Task<SuatChieu> CreateAsync(SuatChieu suatChieu)
    {
        _context.SuatChieus.Add(suatChieu);
        await _context.SaveChangesAsync();
        return suatChieu;
    }

    public async Task<bool> UpdateAsync(SuatChieu suatChieu)
    {
        _context.SuatChieus.Update(suatChieu);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var e = await _context.SuatChieus.FindAsync(id);
        if (e is null) return false;
        _context.SuatChieus.Remove(e);
        return await _context.SaveChangesAsync() > 0;
    }
}
