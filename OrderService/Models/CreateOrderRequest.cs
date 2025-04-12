using System.Collections.Generic;

namespace OrderService.Models
{
    public class CreateOrderRequest
    {
        public string UserId { get; set; }                   // User who is placing the order
        public string ShippingAddress { get; set; }          // Shipping address details
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public string MedicationId { get; set; }             // Medication identifier
        public string MedicationName { get; set; }           // Medication name
        public int Quantity { get; set; }                    // Quantity of the medication to order
        public decimal Price { get; set; }                   // Price per unit of the medication
    }
}
