using CineTicket.Models.VNPay;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CineTicket.Services.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

        // NEW: xác nhận kết quả và cập nhật DB (idempotent)
        Task<(bool ok, int? maHd, decimal? amount, string? orderRef, string message)>
            ConfirmAndSettleAsync(IQueryCollection query);
    }
}
