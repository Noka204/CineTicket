using CineTicket.Models.Momo;
using CineTicket.Models.Order;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CineTicket.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
        Task<bool> ConfirmByQueryAsync(MomoNotifyRequestModel model);
        bool VerifySignature(MomoNotifyRequestModel body);
    }
}
