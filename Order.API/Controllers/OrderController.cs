using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.ViewModels;
using Shared.Events;
using Shared.Messages;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderAPIDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderController(OrderAPIDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderVM createOrder)
        {
            Order.API.Models.Entities.Order order = new()
            {
                BuyerId = createOrder.BuyerId,
                CreatedDate = DateTime.Now,
                OrderStatus = Models.Enums.OrderStatus.Suspend,
                OrderId = Guid.NewGuid()
            };

            order.OrderItems = createOrder.OrderItems.Select(oi => new OrderItem
            {
                ProductId = oi.ProductId,
                Count = oi.Count,
                Price = oi.Price,
            }).ToList();

            order.TotalPrice = createOrder.OrderItems.Sum(oi => (oi.Price * oi.Count));

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId = order.BuyerId,
                OrderId = order.OrderId,
                OrderItems = order.OrderItems.Select(oi => new OrderItemMessage
                {
                    Count = oi.Count,
                    ProductId = oi.ProductId
                }).ToList(),
                TotalPrice = order.TotalPrice,
               
            };
            await _publishEndpoint.Publish(orderCreatedEvent); 

            return Ok();
        }
    }
}
