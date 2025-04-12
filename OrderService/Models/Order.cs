using System;
using System.Collections.Generic;
namespace OrderService.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }                     // Unique identifier for the order
        public string UserId { get; set; }                    // Identifier of the user placing the order
        public DateTime OrderDate { get; set; }               // Date and time when the order was placed
        public string ShippingAddress { get; set; }           // Shipping address details
        public decimal TotalAmount { get; set; }              // Total amount for the order
        public string PaymentTransactionId { get; set; }      // Transaction ID from Payment Service
        public string PaymentStatus { get; set; }             // e.g., Pending, Completed, Failed
        public List<OrderItem> Items { get; set; } = new();   // List of items included in the order
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }                  // Primary key for the order item
        public Guid OrderId { get; set; }                     // Foreign key referencing the Order
        public string MedicationId { get; set; }              // Identifier for the medication
        public string MedicationName { get; set; }            // Name of the medication
        public int Quantity { get; set; }                     // Quantity ordered
        public decimal Price { get; set; }                    // Price per unit at the time of order
    }
}
