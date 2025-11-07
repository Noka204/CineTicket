namespace CineTicket.Models.VNPay;

public class PaymentInformationModel
{
    public string OrderId { get; set; } = string.Empty; // vnp_TxnRef
    public string OrderType { get; set; } = "other";   // vnp_OrderType
    public double Amount { get; set; }                 // VND (not x100 here)
    public string OrderDescription { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Locale { get; set; } = "vn";
    public string? BankCode { get; set; }
    public int? ExpireMinutes { get; set; }
}
