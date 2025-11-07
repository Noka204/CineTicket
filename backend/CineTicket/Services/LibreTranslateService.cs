// LibreTranslateService.cs (relaxed + auto-source + smart fallback)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CineTicket.Services
{
    public interface ILibreTranslateService
    {
        Task<string> TranslateAsync(
            string text,
            string target,
            string source = "vi",
            CancellationToken ct = default);

        Task<IReadOnlyList<string>> TranslateManyAsync(
            IReadOnlyList<string> texts,
            string target,
            string source = "vi",
            CancellationToken ct = default);
    }

    public sealed class LibreTranslateService : ILibreTranslateService
    {
        private readonly IHttpClientFactory _http;
        public LibreTranslateService(IHttpClientFactory http) => _http = http;

        // === Ngưỡng nới lỏng: ít cắt hơn, batch to hơn (nếu server chịu được) ===
        private const int MAX_TOTAL_CHARS = 200_000;
        private const int MAX_LEN_PER_CHUNK = 600;   // tăng so với trước
        private const int BATCH_SIZE = 80;    // tăng nhẹ

        // Regex compiled sẵn
        private static readonly Regex RX_PARA = new(@"\n{2,}", RegexOptions.Compiled);
        private static readonly Regex RX_SENT = new(@"(?<=[\.\!\?\;\:])\s+", RegexOptions.Compiled);
        private static readonly Regex RX_SPACES = new(@"[ \t]{2,}", RegexOptions.Compiled);
        private static readonly Regex RX_EOS = new(@"[\.!\?:;\)]\s*$", RegexOptions.Compiled);

        private sealed class LibreItem { public string? translatedText { get; set; } }

        // Nhẹ tay: “auto” trước, nếu nghi ngờ chưa dịch thì thử “vi”
        private const bool PREFER_AUTO_SOURCE = true;

        public async Task<string> TranslateAsync(
            string text,
            string target,
            string source = "vi",
            CancellationToken ct = default)
        {
            text ??= string.Empty;
            target = NormalizeLang(target);
            source = NormalizeLang(source);

            if (string.IsNullOrWhiteSpace(text) || SameLang(source, target))
                return text;

            var clean = Sanitize(text);
            if (clean.Length > MAX_TOTAL_CHARS) clean = clean[..MAX_TOTAL_CHARS];

            // 1) Nếu chuỗi không quá dài → thử 1 phát nguyên khối (ít lỗi hơn)
            if (clean.Length <= 5000)
            {
                var s1 = PREFER_AUTO_SOURCE ? "auto" : source;
                var res = await TranslateRawOnce(clean, s1, target, ct).ConfigureAwait(false);

                if (LooksUntranslated(clean, res, target))
                {
                    // Fallback thử với source gốc “vi” (hoặc chính source)
                    res = await TranslateRawOnce(clean, source, target, ct).ConfigureAwait(false);
                }

                return string.IsNullOrEmpty(res) ? clean : res;
            }

            // 2) Dài thì chunk mượt
            var parts = ChunkHierarchy(clean, MAX_LEN_PER_CHUNK);
            if (parts.Count == 0) return clean;

            var translatedPieces = new List<string>(parts.Count);

            // Ưu tiên auto-source
            for (int i = 0; i < parts.Count; i += BATCH_SIZE)
            {
                var take = Math.Min(BATCH_SIZE, parts.Count - i);
                var batch = parts.GetRange(i, take);
                var res = await TranslateBatchSafeAsync(batch, "auto", target, ct).ConfigureAwait(false);
                translatedPieces.AddRange(res);
            }

            var joined = Rejoin(parts, translatedPieces);
            if (!LooksUntranslated(clean, joined, target))
                return string.IsNullOrWhiteSpace(joined) ? clean : joined;

            // 3) Fallback: thử lại toàn bộ bằng source rõ ràng (vi) theo batch
            translatedPieces.Clear();
            for (int i = 0; i < parts.Count; i += BATCH_SIZE)
            {
                var take = Math.Min(BATCH_SIZE, parts.Count - i);
                var batch = parts.GetRange(i, take);
                var res = await TranslateBatchSafeAsync(batch, source, target, ct).ConfigureAwait(false);
                translatedPieces.AddRange(res);
            }

            var joined2 = Rejoin(parts, translatedPieces);
            return string.IsNullOrWhiteSpace(joined2) ? clean : joined2;
        }

        public async Task<IReadOnlyList<string>> TranslateManyAsync(
            IReadOnlyList<string> texts,
            string target,
            string source = "vi",
            CancellationToken ct = default)
        {
            target = NormalizeLang(target);
            source = NormalizeLang(source);

            if (texts == null || texts.Count == 0 || SameLang(source, target))
                return texts ?? Array.Empty<string>();

            var results = new string[texts.Count];

            for (int i = 0; i < texts.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var raw = texts[i] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(raw)) { results[i] = raw; continue; }

                var clean = Sanitize(raw);
                if (clean.Length > MAX_TOTAL_CHARS) clean = clean[..MAX_TOTAL_CHARS];

                if (clean.Length <= 5000)
                {
                    var s1 = PREFER_AUTO_SOURCE ? "auto" : source;
                    var res = await TranslateRawOnce(clean, s1, target, ct).ConfigureAwait(false);
                    if (LooksUntranslated(clean, res, target))
                    {
                        res = await TranslateRawOnce(clean, source, target, ct).ConfigureAwait(false);
                    }
                    results[i] = string.IsNullOrEmpty(res) ? clean : res;
                    continue;
                }

                // dài → chunk
                var parts = ChunkHierarchy(clean, MAX_LEN_PER_CHUNK);
                if (parts.Count == 0) { results[i] = clean; continue; }

                var translatedPieces = new List<string>(parts.Count);
                for (int k = 0; k < parts.Count; k += BATCH_SIZE)
                {
                    var take = Math.Min(BATCH_SIZE, parts.Count - k);
                    var batch = parts.GetRange(k, take);
                    var res = await TranslateBatchSafeAsync(batch, "auto", target, ct).ConfigureAwait(false);
                    translatedPieces.AddRange(res);
                }

                var joined = Rejoin(parts, translatedPieces);
                if (LooksUntranslated(clean, joined, target))
                {
                    translatedPieces.Clear();
                    for (int k = 0; k < parts.Count; k += BATCH_SIZE)
                    {
                        var take = Math.Min(BATCH_SIZE, parts.Count - k);
                        var batch = parts.GetRange(k, take);
                        var res = await TranslateBatchSafeAsync(batch, source, target, ct).ConfigureAwait(false);
                        translatedPieces.AddRange(res);
                    }
                    joined = Rejoin(parts, translatedPieces);
                }

                results[i] = string.IsNullOrWhiteSpace(joined) ? clean : joined;
            }

            return results;
        }

        // ========== Low-level calls ==========

        private async Task<string> TranslateRawOnce(
            string text, string source, string target, CancellationToken ct)
        {
            var client = _http.CreateClient("LibreTranslate");
            using var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("q", text),
                new KeyValuePair<string,string>("source", source),
                new KeyValuePair<string,string>("target", target),
                new KeyValuePair<string,string>("format", "text"),
                new KeyValuePair<string,string>("alternatives", "0"),
            });

            try
            {
                using var res = await client.PostAsync("/translate", form, ct).ConfigureAwait(false);
                var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode) return text;

                // object đơn
                try
                {
                    var obj = JsonSerializer.Deserialize<LibreItem>(body);
                    if (obj?.translatedText != null) return obj.translatedText;
                }
                catch { /* ignore */ }

                // mảng (trường hợp server vẫn trả về array)
                try
                {
                    var arr = JsonSerializer.Deserialize<List<LibreItem>>(body);
                    if (arr != null && arr.Count > 0)
                        return arr[0]?.translatedText ?? text;
                }
                catch { /* ignore */ }
            }
            catch { /* lỗi mạng → trả nguyên */ }

            return text;
        }

        private async Task<List<string>> TranslateBatchSafeAsync(
            List<string> parts, string source, string target, CancellationToken ct)
        {
            int size = parts.Count;
            while (size > 0)
            {
                var current = parts.GetRange(0, size);
                var res = await TryTranslateOnce(current, source, target, ct).ConfigureAwait(false);
                if (res != null) return res;
                size >>= 1; // 413/5xx → giảm nửa batch
            }
            return parts;
        }

        private async Task<List<string>?> TryTranslateOnce(
            List<string> parts, string source, string target, CancellationToken ct)
        {
            var client = _http.CreateClient("LibreTranslate");

            IEnumerable<KeyValuePair<string, string>> BuildForm()
            {
                foreach (var q in parts) yield return new("q", q);
                yield return new("source", source);
                yield return new("target", target);
                yield return new("format", "text");
                yield return new("alternatives", "0");
            }

            using var form = new FormUrlEncodedContent(BuildForm());

            try
            {
                using var res = await client.PostAsync("/translate", form, ct).ConfigureAwait(false);
                var code = (int)res.StatusCode;
                var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (res.StatusCode == HttpStatusCode.RequestEntityTooLarge) return null;
                if (!res.IsSuccessStatusCode) return code >= 500 ? null : parts;

                // mảng nhiều q=
                try
                {
                    var arr = JsonSerializer.Deserialize<List<LibreItem>>(body);
                    if (arr is { Count: > 0 })
                        return arr.Select(x => x?.translatedText ?? string.Empty).ToList();
                }
                catch { /* ignore */ }

                // object đơn → replicate
                try
                {
                    var obj = JsonSerializer.Deserialize<LibreItem>(body);
                    if (obj?.translatedText != null)
                        return Enumerable.Repeat(obj.translatedText, parts.Count).ToList();
                }
                catch { /* ignore */ }

                return parts;
            }
            catch
            {
                return null;
            }
        }

        // ========== Text utils ==========

        private static bool SameLang(string? src, string? tgt)
        {
            var a = NormalizeLang(src);
            var b = NormalizeLang(tgt);
            return a == b;
        }

        private static string NormalizeLang(string? lang)
        {
            lang = (lang ?? "vi").Trim().ToLowerInvariant();
            return lang switch
            {
                "vi-vn" => "vi",
                "en-us" => "en",
                "fr-fr" => "fr",
                _ => lang
            };
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Replace('\u00A0', ' ')
                 .Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "")
                 .Replace("\uFEFF", "")
                 .Replace('“', '"').Replace('”', '"')
                 .Replace('‘', '\'').Replace('’', '\'')
                 .Replace("\r\n", "\n").Replace("\r", "\n");

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsControl(ch) && ch != '\n' && ch != '\t') continue;
                sb.Append(ch);
            }
            s = sb.ToString();

            s = RX_SPACES.Replace(s, " ");
            return s.Trim();
        }

        private static List<string> ChunkHierarchy(string text, int maxLen)
        {
            var segments = RX_PARA.Split(text).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            var outList = new List<string>(segments.Length * 8);

            foreach (var seg in segments)
            {
                var sents = RX_SENT.Split(seg).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                if (sents.Length == 0) ChunkSoft(seg, maxLen, outList);
                else
                {
                    foreach (var sent in sents)
                    {
                        if (sent.Length <= maxLen) outList.Add(sent);
                        else ChunkSoft(sent, maxLen, outList);
                    }
                }
                outList.Add("\n\n");
            }
            if (outList.Count > 0 && outList[^1] == "\n\n") outList.RemoveAt(outList.Count - 1);
            return outList;
        }

        private static void ChunkSoft(string s, int maxLen, List<string> output)
        {
            var cur = s.AsSpan();
            while (cur.Length > maxLen)
            {
                int limit = Math.Min(maxLen, cur.Length);
                int cut = LastGoodCut(cur, limit);
                output.Add(cur[..cut].ToString().TrimEnd());
                cur = cur[cut..].TrimStart();
            }
            if (!cur.IsEmpty) output.Add(cur.ToString());
        }

        private static int LastGoodCut(ReadOnlySpan<char> s, int limit)
        {
            int cut = limit;
            for (int i = limit - 1; i >= 0; i--)
            {
                char c = s[i];
                if (c is '.' or '!' or '?' or ';' or ':' or ',' or ' ')
                {
                    cut = i + 1;
                    break;
                }
            }
            if (cut < 120) cut = limit;
            return cut;
        }

        private static string Rejoin(List<string> orig, List<string> trans)
        {
            int n = Math.Min(orig.Count, trans.Count);
            int approx = 0; for (int i = 0; i < n; i++) approx += trans[i]?.Length ?? 0;
            var sb = new StringBuilder(Math.Max(approx + orig.Count, 16));

            for (int i = 0; i < n; i++)
            {
                var src = orig[i];
                if (src == "\n\n") { sb.Append("\n\n"); continue; }

                var dst = trans[i] ?? string.Empty;
                sb.Append(dst);

                bool nextIsPara = (i + 1 < orig.Count) && orig[i + 1] == "\n\n";
                if (!nextIsPara && i + 1 < n)
                {
                    if (!RX_EOS.IsMatch(dst)) sb.Append(' ');
                }
            }
            return sb.ToString().Trim();
        }

        // —— Heuristic nhẹ: nếu đích là en/fr mà vẫn còn nhiều dấu tiếng Việt → coi như chưa dịch
        private static bool LooksUntranslated(string src, string dst, string target)
        {
            if (string.IsNullOrWhiteSpace(dst)) return true;
            target = NormalizeLang(target);
            if (target != "en" && target != "fr") return false;

            // nếu giống nhau y xì → chưa dịch
            if (string.Equals(src.Trim(), dst.Trim(), StringComparison.Ordinal)) return true;

            // nếu còn nhiều ký tự có dấu tiếng Việt
            int viMarks = 0, letters = 0;
            foreach (var ch in dst)
            {
                if (char.IsLetter(ch)) letters++;
                if ("ăâđêôơưáàạảãấầậẩẫắằặẳẵéèẹẻẽếềệểễóòọỏõốồộổỗớờợởỡíìịỉĩúùụủũứừựửữýỳỵỷỹ".IndexOf(char.ToLowerInvariant(ch)) >= 0)
                    viMarks++;
            }
            if (letters >= 50 && viMarks * 100 / Math.Max(letters, 1) >= 15) return true; // ≥15% có dấu

            return false;
        }
    }
}
