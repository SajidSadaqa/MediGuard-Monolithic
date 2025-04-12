using System;

namespace SharedModels.Models
{
    /// <summary>
    /// Represents basic user information shared across services.
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// Represents an address, for shipping or billing.
    /// </summary>
    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

    /// <summary>
    /// Represents medication details that may be used in various contexts.
    /// </summary>
    public class MedicationDto
    {
        public string MedicationId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Provides a summary of an order for display across services.
    /// </summary>
    public class OrderSummaryDto
    {
        public Guid OrderId { get; set; }
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
