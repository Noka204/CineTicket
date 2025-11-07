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
        private readonly ILibreTranslateService _trans;

        public BapNuocService(IBapNuocRepository repo, ILibreTranslateService trans)
        {
            _repo = repo;
            _trans = trans;
        }

        public async Task<IEnumerable<BapNuocDTO>> GetAllAsync(string targetLang = "vi", CancellationToken ct = default)
        {
            var entities = await _repo.GetAllAsync();
            // map trước
            var dtos = entities.Select(e => new BapNuocDTO
            {
                MaBn = e.MaBn,
                TenBn = e.TenBn,
                Gia = e.Gia,
                MoTa = e.MoTa,
                HinhAnhUrl = e.HinhAnhUrl
            }).ToList();

            // chỉ dịch mô tả nếu target != vi
            if (!string.Equals(targetLang, "vi", StringComparison.OrdinalIgnoreCase))
            {
                // gom các MoTa cần dịch để batch
                var pairs = dtos
                    .Select((x, i) => new { i, text = x.MoTa ?? string.Empty })
                    .Where(p => !string.IsNullOrWhiteSpace(p.text))
                    .ToList();

                if (pairs.Count > 0)
                {
                    // nguồn là "vi" (như DB), nhưng client có auto-fallback trong service
                    var inputs = pairs.Select(p => p.text).ToList();
                    IReadOnlyList<string> outputs;

                    try
                    {
                        outputs = await _trans.TranslateManyAsync(inputs, target: targetLang, source: "vi", ct);
                    }
                    catch
                    {
                        outputs = inputs; // lỗi → trả nguyên văn
                    }

                    for (int k = 0; k < pairs.Count && k < outputs.Count; k++)
                    {
                        var idx = pairs[k].i;
                        dtos[idx].MoTa = string.IsNullOrWhiteSpace(outputs[k]) ? dtos[idx].MoTa : outputs[k];
                    }
                }
            }

            return dtos;
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
