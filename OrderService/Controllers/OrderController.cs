using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("order")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IPaymentServiceClient _paymentServiceClient;

        public OrderController(OrderDbContext context, IPaymentServiceClient paymentServiceClient)
        {
            _context = context;
            _paymentServiceClient = paymentServiceClient;
        }

        /// <summary>
        /// POST /order/ - Creates a new order.
        /// </summary>
        /// <param name="request">Order creation details, including user ID, shipping address, and order items.</param>
        /// <returns>The created order with payment information.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                return BadRequest("Invalid order data provided.");

            // Calculate total amount based on order items.
            decimal totalAmount = request.Items.Sum(item => item.Price * item.Quantity);

            // Create a new Order.
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = request.UserId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = request.ShippingAddress,
                TotalAmount = totalAmount,
                PaymentStatus = "Pending",
                Items = request.Items.Select(i => new OrderItem
                {
                    MedicationId = i.MedicationId,
                    MedicationName = i.MedicationName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            // Save the order to the database.
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Initiate payment processing via the Payment Service.
            PaymentResult paymentResult = await _paymentServiceClient.ProcessPaymentAsync(order.OrderId, totalAmount);
            if (paymentResult.IsSuccess)
            {
                order.PaymentTransactionId = paymentResult.TransactionId;
                order.PaymentStatus = "Completed";
            }
            else
            {
                order.PaymentStatus = "Failed";
            }

            // Update the order with payment information.
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // Return the created order details.
            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, order);
        }

        /// <summary>
        /// GET /order/{orderId} - Retrieves specific order details by order ID.
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound($"Order with ID '{orderId}' was not found.");

            return Ok(order);
        }

        /// <summary>
        /// GET /order/user/{userId} - Retrieves past orders for a specific user.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUser(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }
    }
}
