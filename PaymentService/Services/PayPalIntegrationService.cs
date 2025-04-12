using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Models;
using PaymentService.Data;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Services
{
    public interface IPayPalIntegrationService
    {
        Task<PaymentTransaction> InitiatePaymentAsync(Guid orderId, decimal amount);
        Task<PaymentTransaction> ConfirmPaymentAsync(Guid transactionId);
    }

    public class PayPalIntegrationService : IPayPalIntegrationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayPalIntegrationService> _logger;
        private readonly PaymentDbContext _dbContext;

        public PayPalIntegrationService(
            IConfiguration configuration,
            ILogger<PayPalIntegrationService> logger,
            PaymentDbContext dbContext)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<PaymentTransaction> InitiatePaymentAsync(Guid orderId, decimal amount)
        {
            var transaction = new PaymentTransaction
            {
                TransactionId = Guid.NewGuid(),
                OrderId = orderId,
                Amount = amount,
                PaymentStatus = "Pending",
                TransactionDate = DateTime.UtcNow
            };

            _logger.LogInformation($"Payment initiated for Order {orderId} with amount {amount}.");
            return await Task.FromResult(transaction); // In real case, integrate PayPal API here
        }

        public async Task<PaymentTransaction> ConfirmPaymentAsync(Guid transactionId)
        {
            var transaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                _logger.LogWarning($"Transaction {transactionId} not found for confirmation.");
                throw new InvalidOperationException("Transaction not found.");
            }

            transaction.PaymentStatus = "Completed";
            transaction.TransactionDate = DateTime.UtcNow;

            _logger.LogInformation($"Payment confirmed for Transaction {transactionId}.");
            return transaction; // In real case, confirm with PayPal API
        }
    }
}