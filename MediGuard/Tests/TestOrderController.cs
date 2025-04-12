using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MediGuard.API.Controllers;
using MediGuard.API.DTOs;
using MediGuard.API.Services;
using MediGuard.API.Helpers;
using System.Security.Claims;
using Xunit;

namespace MediGuard.Tests
{
    public class TestOrderController
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<ILogger<OrderController>> _mockLogger;
        private readonly OrderController _controller;

        public TestOrderController()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrderController>>();
            _controller = new OrderController(_mockOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUserOrders_ReturnsOkResult()
        {
            // Arrange
            var userId = "user-id";
            var orders = new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    TotalAmount = 15.97m,
                    Status = "Delivered",
                    ShippingAddress = "123 Test St, Test City",
                    PaymentMethod = "Credit Card",
                    PaymentTransactionId = "TXN-123456",
                    ShippedDate = DateTime.UtcNow.AddDays(-4),
                    DeliveredDate = DateTime.UtcNow.AddDays(-2),
                    OrderItems = new List<OrderItemDto>
                    {
                        new OrderItemDto
                        {
                            Id = 1,
                            MedicationId = 1,
                            MedicationName = "Advil",
                            Quantity = 2,
                            UnitPrice = 5.99m,
                            Subtotal = 11.98m
                        },
                        new OrderItemDto
                        {
                            Id = 2,
                            MedicationId = 2,
                            MedicationName = "Tylenol",
                            Quantity = 1,
                            UnitPrice = 3.99m,
                            Subtotal = 3.99m
                        }
                    }
                },
                new OrderDto
                {
                    Id = 2,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    TotalAmount = 8.99m,
                    Status = "Processing",
                    ShippingAddress = "123 Test St, Test City",
                    PaymentMethod = "Credit Card",
                    PaymentTransactionId = "TXN-789012",
                    OrderItems = new List<OrderItemDto>
                    {
                        new OrderItemDto
                        {
                            Id = 3,
                            MedicationId = 3,
                            MedicationName = "Warfarin",
                            Quantity = 1,
                            UnitPrice = 8.99m,
                            Subtotal = 8.99m
                        }
                    }
                }
            };

            _mockOrderService.Setup(service => service.GetUserOrdersAsync(userId))
                .ReturnsAsync(orders);

            // Mock the User.FindFirstValue to return the userId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetUserOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<OrderDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task CreateOrder_ValidInput_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var userId = "user-id";
            var createOrderDto = new CreateOrderDto
            {
                ShippingAddress = "123 Test St, Test City",
                PaymentMethod = "Credit Card",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        MedicationId = 1,
                        Quantity = 2
                    },
                    new CreateOrderItemDto
                    {
                        MedicationId = 2,
                        Quantity = 1
                    }
                }
            };

            var createdOrder = new OrderDto
            {
                Id = 3,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 15.97m,
                Status = "Processing",
                ShippingAddress = "123 Test St, Test City",
                PaymentMethod = "Credit Card",
                PaymentTransactionId = "TXN-345678",
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = 4,
                        MedicationId = 1,
                        MedicationName = "Advil",
                        Quantity = 2,
                        UnitPrice = 5.99m,
                        Subtotal = 11.98m
                    },
                    new OrderItemDto
                    {
                        Id = 5,
                        MedicationId = 2,
                        MedicationName = "Tylenol",
                        Quantity = 1,
                        UnitPrice = 3.99m,
                        Subtotal = 3.99m
                    }
                }
            };

            _mockOrderService.Setup(service => service.CreateOrderAsync(userId, It.IsAny<CreateOrderDto>()))
                .ReturnsAsync(createdOrder);

            // Mock the User.FindFirstValue to return the userId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<OrderDto>(createdAtActionResult.Value);
            Assert.Equal(3, returnValue.Id);
            Assert.Equal(15.97m, returnValue.TotalAmount);
            Assert.Equal(2, returnValue.OrderItems.Count);
        }
    }
}
