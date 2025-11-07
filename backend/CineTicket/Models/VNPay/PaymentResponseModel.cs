namespace CineTicket.Models.VNPay;

public class PaymentResponseModel
{
    public string OrderDescription { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;   // vnp_TransactionNo
    public string OrderId { get; set; } = string.Empty;         // vnp_TxnRef
    public string PaymentMethod { get; set; } = "VnPay";
    public string PaymentId { get; set; } = string.Empty;       // alias
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;           // vnp_SecureHash
    public string VnPayResponseCode { get; set; } = string.Empty; // vnp_ResponseCode
}
