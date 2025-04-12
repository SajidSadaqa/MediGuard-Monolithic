using System;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IPaymentServiceClient
    {
        /// <summary>
        /// Processes a payment for the given order.
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <param name="amount">The total amount to charge</param>
        /// <returns>A PaymentResult indicating success or failure along with a transaction ID.</returns>
        Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount);
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }                // Indicates if the payment was successful
        public string TransactionId { get; set; }            // Transaction ID returned from Payment Service
    }
}
