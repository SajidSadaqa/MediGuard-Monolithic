using System.Threading.Tasks;

namespace MediGuard.API.Helpers
{
    public partial interface IDummyPaymentProcessor
    {
        /// <summary>
        /// Simulates processing a payment.
        /// </summary>
        /// <param name="orderId">Order identifier.</param>
        /// <param name="amount">Payment amount.</param>
        /// <param name="paymentMethod">Payment method string.</param>
        /// <returns>A PaymentResult object describing the outcome.</returns>
        Task<PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, string paymentMethod);

        /// <summary>
        /// Simulates processing a refund.
        /// </summary>
        /// <param name="transactionId">Original payment transaction ID.</param>
        /// <param name="amount">Amount to be refunded.</param>
        /// <returns>A PaymentResult object describing the refund outcome.</returns>
        Task<PaymentResult> ProcessRefundAsync(string transactionId, decimal amount);
    }
}
