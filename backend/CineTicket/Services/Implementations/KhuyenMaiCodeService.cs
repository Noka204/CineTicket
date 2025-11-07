using CineTicket.Data;
using CineTicket.DTOs.KhuyenMai;
using CineTicket.Models;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CineTicket.Services.Implementations
{
    public class KhuyenMaiCodeService : IKhuyenMaiCodeService
    {
        private readonly IKhuyenMaiCodeRepository _repo;
        private readonly IKhuyenMaiRepository _kmRepo;
        private readonly CineTicketDbContext _db;

        public KhuyenMaiCodeService(
            IKhuyenMaiCodeRepository repo,
            IKhuyenMaiRepository kmRepo,
            CineTicketDbContext db)
        {
            _repo = repo; _kmRepo = kmRepo; _db = db;
        }

        public Task<List<KhuyenMaiCode>> GetByPromotionAsync(int khuyenMaiId) =>
            _repo.GetByPromotionAsync(khuyenMaiId);

        public Task<KhuyenMaiCode?> GetByIdAsync(int id) =>
            _repo.GetByIdAsync(id);

        public async Task<KhuyenMaiCode> CreateAsync(KhuyenMaiCodeCreateDto dto)
        {
            // nếu có Count => chuyển sang bulk
            if ((dto.Count ?? 0) > 0)
                throw new InvalidOperationException("Dùng BulkGenerateAsync để tạo loạt mã.");

            // validate tồn tại KM
            var km = await _kmRepo.GetAsync(dto.KhuyenMaiId)
                     ?? throw new InvalidOperationException("Khuyến mãi không tồn tại.");

            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new InvalidOperationException("Thiếu Code.");

            var entity = new KhuyenMaiCode
            {
                KhuyenMaiId = km.Id,
                Code = dto.Code!.Trim().ToUpperInvariant(),
                AssignedToUserId = dto.AssignedToUserId
            };

            await _repo.AddAsync(entity);
            return entity;
        }

        public async Task<List<KhuyenMaiCode>> BulkGenerateAsync(int khuyenMaiId, int count, string? prefix, string? assignedToUserId)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            var km = await _kmRepo.GetAsync(khuyenMaiId)
                     ?? throw new InvalidOperationException("Khuyến mãi không tồn tại.");

            string pre = string.IsNullOrWhiteSpace(prefix) ? "KM" : prefix!.Trim().ToUpperInvariant();
            var rand = new Random();

            var result = new List<KhuyenMaiCode>(count);
            var candidateSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // tránh đụng unique index: kiểm tra đợt hiện tại + trong DB
            while (result.Count < count)
            {
                var code = $"{pre}-{RandomToken(rand, 8)}";
                if (candidateSet.Contains(code)) continue;

                bool exists = await _db.KhuyenMaiCodes.AnyAsync(x => x.Code == code);
                if (exists) continue;

                candidateSet.Add(code);
                result.Add(new KhuyenMaiCode
                {
                    KhuyenMaiId = km.Id,
                    AssignedToUserId = assignedToUserId,
                    Code = code
                });
            }

            await _repo.AddBulkAsync(result);
            return result;

            static string RandomToken(Random r, int len)
            {
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                return new string(Enumerable.Range(0, len)
                    .Select(_ => chars[r.Next(chars.Length)]).ToArray());
            }
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
