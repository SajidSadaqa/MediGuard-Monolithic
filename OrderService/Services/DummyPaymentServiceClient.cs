using System;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class DummyPaymentServiceClient : IPaymentServiceClient
    {
        public Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount)
        {
            // Simulate a successful payment response.
            var result = new PaymentResult
            {
                IsSuccess = true,
                TransactionId = Guid.NewGuid().ToString()
            };

            return Task.FromResult(result);
        }
    }
}
