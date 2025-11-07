// Localization/SimpleLanguageProvider.cs
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;

namespace CineTicket.Resources.Localization
{
    public sealed class SimpleLanguageProvider : IRequestCultureProvider
    {
        private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["vi"] = "vi-VN",
            ["vietnamese"] = "vi-VN",
            ["tiếng việt"] = "vi-VN",
            ["en"] = "en-US",
            ["english"] = "en-US",
            ["fr"] = "fr-FR",
            ["french"] = "fr-FR",
            ["français"] = "fr-FR"
        };

        public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext ctx)
        {
            if (ctx.Request.Query.TryGetValue("lang", out StringValues q) && TryMap(q, out var r))
                return Task.FromResult(r);
            if (ctx.Request.Headers.TryGetValue("X-Lang", out var h) && TryMap(h, out r))
                return Task.FromResult(r);
            return Task.FromResult<ProviderCultureResult?>(null);

            static bool TryMap(StringValues v, out ProviderCultureResult? r)
            {
                var s = v.ToString().Trim();
                if (!string.IsNullOrEmpty(s) && Map.TryGetValue(s, out var culture))
                {
                    r = new ProviderCultureResult(culture, culture);
                    return true;
                }
                r = null; return false;
            }
        }
    }
}
