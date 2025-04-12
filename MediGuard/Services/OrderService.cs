using MediGuard.API.Data;
using MediGuard.API.DTOs;
using MediGuard.API.Helpers;
using MediGuard.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediGuard.API.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetUserOrdersAsync(string userId);
        Task<OrderDto?> GetOrderByIdAsync(int id, string userId);
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto orderDto);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> CancelOrderAsync(int id, string userId);
    }

    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly DummyPaymentProcessor _paymentProcessor;

        public OrderService(
            AppDbContext context, 
            ILogger<OrderService> logger,
            IDummyPaymentProcessor paymentProcessor)
        {
            _context = context;
            _logger = logger;
            _paymentProcessor = (DummyPaymentProcessor?)paymentProcessor;
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Medication)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Medication)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            return order != null ? MapToDto(order) : null;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto orderDto)
        {
            // Validate order items
            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
            {
                throw new ApplicationException("Order must contain at least one item");
            }

            // Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create new order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    ShippingAddress = orderDto.ShippingAddress,
                    PaymentMethod = orderDto.PaymentMethod
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                // Add order items
                foreach (var item in orderDto.OrderItems)
                {
                    var medication = await _context.Medications.FindAsync(item.MedicationId);
                    if (medication == null)
                    {
                        throw new ApplicationException($"Medication with ID {item.MedicationId} not found");
                    }

                    if (!medication.IsAvailable)
                    {
                        throw new ApplicationException($"Medication {medication.Name} is not available");
                    }

                    var subtotal = medication.Price * item.Quantity;
                    totalAmount += subtotal;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        MedicationId = medication.Id,
                        Quantity = item.Quantity,
                        UnitPrice = medication.Price,
                        Subtotal = subtotal
                    };

                    _context.OrderItems.Add(orderItem);
                }

                // Update order total
                order.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();

                // Process payment
                var paymentResult = await _paymentProcessor.ProcessPaymentAsync(
                    order.Id.ToString(),
                    totalAmount,
                    orderDto.PaymentMethod ?? "Credit Card");

                if (paymentResult.Success)
                {
                    order.PaymentTransactionId = paymentResult.TransactionId;
                    order.Status = "Processing";
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    throw new ApplicationException($"Payment failed: {paymentResult.Message}");
                }

                // Load the complete order with items for return
                var completeOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medication)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                return MapToDto(completeOrder!);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return false;
            }

            // Validate status transition
            if (!IsValidStatusTransition(order.Status, status))
            {
                throw new ApplicationException($"Invalid status transition from {order.Status} to {status}");
            }

            order.Status = status;

            // Update additional fields based on status
            if (status == "Shipped")
            {
                order.ShippedDate = DateTime.UtcNow;
            }
            else if (status == "Delivered")
            {
                order.DeliveredDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int id, string userId)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.UserId != userId)
            {
                return false;
            }

            // Only pending or processing orders can be cancelled
            if (order.Status != "Pending" && order.Status != "Processing")
            {
                throw new ApplicationException($"Cannot cancel order with status {order.Status}");
            }

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();

            // Process refund if payment was made
            if (!string.IsNullOrEmpty(order.PaymentTransactionId))
            {
                await _paymentProcessor.ProcessRefundAsync(order.PaymentTransactionId, order.TotalAmount);
            }

            return true;
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                PaymentTransactionId = order.PaymentTransactionId,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    MedicationId = oi.MedicationId,
                    MedicationName = oi.Medication?.Name ?? "Unknown",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal
                }).ToList()
            };
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                ("Pending", "Processing") => true,
                ("Pending", "Cancelled") => true,
                ("Processing", "Shipped") => true,
                ("Processing", "Cancelled") => true,
                ("Shipped", "Delivered") => true,
                _ => false
            };
        }
    }
}
