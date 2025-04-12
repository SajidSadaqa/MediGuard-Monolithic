using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;
using PaymentService.Services;
using PaymentService.Data;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPayPalIntegrationService _payPalService;
        private readonly PaymentDbContext _dbContext;

        public PaymentController(
            IPayPalIntegrationService payPalService,
            PaymentDbContext dbContext)
        {
            _payPalService = payPalService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// POST /payment/paypal - Initiates a PayPal payment and stores it.
        /// </summary>
        [HttpPost("paypal")]
        public async Task<IActionResult> InitiatePayPalPayment([FromBody] InitiatePaymentRequest request)
        {
            if (request == null || request.OrderId == Guid.Empty || request.Amount <= 0)
                return BadRequest(new { Message = "Invalid payment request." });

            var transaction = await _payPalService.InitiatePaymentAsync(request.OrderId, request.Amount);
            _dbContext.PaymentTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(transaction);
        }

        /// <summary>
        /// POST /payment/paypal/confirm - Confirms and updates the payment status.
        /// </summary>
        [HttpPost("paypal/confirm")]
        public async Task<IActionResult> ConfirmPayPalPayment([FromBody] ConfirmPaymentRequest request)
        {
            if (request == null || request.TransactionId == Guid.Empty)
                return BadRequest(new { Message = "Invalid confirmation request." });

            var existingTransaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TransactionId == request.TransactionId);

            if (existingTransaction == null)
                return NotFound(new { Message = $"Transaction {request.TransactionId} not found." });

            var updatedTransaction = await _payPalService.ConfirmPaymentAsync(request.TransactionId);
            existingTransaction.PaymentStatus = updatedTransaction.PaymentStatus;
            existingTransaction.TransactionDate = updatedTransaction.TransactionDate;

            await _dbContext.SaveChangesAsync();
            return Ok(existingTransaction);
        }

        /// <summary>
        /// GET /payment/{transactionId} - Retrieves a payment transaction by ID.
        /// </summary>
        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetPaymentTransaction(Guid transactionId)
        {
            var transaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
                return NotFound(new { Message = $"Transaction {transactionId} not found." });

            return Ok(transaction);
        }
    }

    public class InitiatePaymentRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public Guid TransactionId { get; set; }
    }
}