using System;
using System.Threading.Tasks;

namespace MediGuard.API.Helpers
{
    public class DummyPaymentProcessor : IDummyPaymentProcessor
    {
        public async Task<PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, string paymentMethod)
        {
            // Simulate a processing delay
            await Task.Delay(500);

            // Basic validation logic
            if (amount <= 0)
            {
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Message = "Payment failed due to invalid amount."
                };
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Message = "Payment failed due to missing payment method."
                };
            }

            // Dummy transaction ID
            string transactionId = Guid.NewGuid().ToString();

            return new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                Message = "Payment processed successfully."
            };
        }

        public async Task<PaymentResult> ProcessRefundAsync(string transactionId, decimal amount)
        {
            // Simulate a processing delay
            await Task.Delay(500);

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Message = "Refund failed: invalid transaction ID."
                };
            }

            if (amount <= 0)
            {
                return new PaymentResult
                {
                    Success = false,
                    TransactionId = null,
                    Message = "Refund failed: amount must be greater than zero."
                };
            }

            // Generate a dummy refund transaction ID
            string refundId = "REF-" + Guid.NewGuid().ToString();

            return new PaymentResult
            {
                Success = true,
                TransactionId = refundId,
                Message = "Refund processed successfully."
            };
        }
    }
}
