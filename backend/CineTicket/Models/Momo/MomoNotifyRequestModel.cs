using System.Text.Json.Serialization;

public class MomoNotifyRequestModel
{
    public string partnerCode { get; set; }
    public string orderId { get; set; }
    public string requestId { get; set; }

    public long amount { get; set; }  // số
    public string orderInfo { get; set; }
    public string orderType { get; set; }
    public long transId { get; set; }  // số
    public int resultCode { get; set; }  // số
    public string message { get; set; }
    public string payType { get; set; }
    public long responseTime { get; set; }  // số
    public string extraData { get; set; }
    public string signature { get; set; }

    // có thể không có, để nullable
    public string? accessKey { get; set; }
}
