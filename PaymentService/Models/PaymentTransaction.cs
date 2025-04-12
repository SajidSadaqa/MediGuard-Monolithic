using System;

namespace PaymentService.Models
{
    public class PaymentTransaction
    {
        /// <summary>
        /// Unique identifier for the payment transaction.
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Identifier of the associated order.
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Total amount processed in this transaction.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The status of the payment (e.g., Pending, Completed, Failed).
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Date and time when the transaction was created.
        /// </summary>
        public DateTime TransactionDate { get; set; }
    }
}
