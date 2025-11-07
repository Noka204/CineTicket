using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CineTicket.Models.VNPay;

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value)) _requestData.Add(key, value);
    }
    public void AddResponseData(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value)) _responseData.Add(key, value);
    }

    public string GetResponseData(string key)
        => _responseData.TryGetValue(key, out var v) ? v : string.Empty;

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        var encodedPairs = new StringBuilder();
        foreach (var (k, v) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            encodedPairs.Append(WebUtility.UrlEncode(k)).Append('=').Append(WebUtility.UrlEncode(v)).Append('&');

        var querystring = encodedPairs.ToString();
        if (querystring.Length > 0) querystring = querystring.Remove(querystring.Length - 1);


        var vnpSecureHash = HmacSha512(vnpHashSecret, querystring);

        var url = baseUrl + "?" + querystring + "&vnp_SecureHash=" + vnpSecureHash;
        return url;
    }

    public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
    {
        var vnPay = new VnPayLibrary();
        foreach (var (key, value) in collection)
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_")) vnPay.AddResponseData(key, value);

        var orderId = vnPay.GetResponseData("vnp_TxnRef");
        var vnPayTranId = vnPay.GetResponseData("vnp_TransactionNo");
        var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
        var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value.ToString();
        var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
        var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);

        return new PaymentResponseModel
        {
            Success = checkSignature,
            PaymentMethod = "VnPay",
            OrderDescription = orderInfo,
            OrderId = orderId,
            PaymentId = vnPayTranId,
            TransactionId = vnPayTranId,
            Token = vnpSecureHash,
            VnPayResponseCode = vnpResponseCode
        };
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseDataRaw();
        var myChecksum = HmacSha512(secretKey, rspRaw);
        return string.Equals(myChecksum, inputHash, StringComparison.OrdinalIgnoreCase);
    }

    private string GetResponseDataRaw()
    {
        _responseData.Remove("vnp_SecureHashType");
        _responseData.Remove("vnp_SecureHash");

        var sb = new StringBuilder();
        foreach (var (k, v) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            sb.Append(WebUtility.UrlEncode(k)).Append('=').Append(WebUtility.UrlEncode(v)).Append('&');
        if (sb.Length > 0) sb.Length -= 1; // remove trailing &
        return sb.ToString();
    }

    private static string HmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        var sb = new StringBuilder(hashValue.Length * 2);
        foreach (var b in hashValue) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public static string GetIpAddress(HttpContext context)
    {
        try
        {
            var ip = context.Connection.RemoteIpAddress;
            if (ip is null) return "127.0.0.1";
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                ip = Dns.GetHostEntry(ip).AddressList
                    .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }
            return ip?.ToString() ?? "127.0.0.1";
        }
        catch { return "127.0.0.1"; }
    }
}
