using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Models.Entities;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumers : IConsumer<OrderCreatedEvent>
    {
        IMongoCollection<Stock.API.Models.Entities.Stock> _stockCollection;
         readonly ISendEndpointProvider _sendEndpointProvider;
         readonly IPublishEndpoint _publishEndpoint;
        public OrderCreatedEventConsumers(MongoDBService mongoDBService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _stockCollection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        //OrderCreatedEvent türünden bir event geldiğinde bunu yakala bu implementasyon
        //sırasında uygulanmış olan consume içerisinde gerekli çalışmalar yapılsın.
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new List<bool>();
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                stockResult.Add((await _stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId && x.Count >= orderItem.Count)).Any());
            }
            if(stockResult.TrueForAll(sr=>sr.Equals(true)))
            {
                //gerekli sipariş işlemleri
               
                foreach (OrderItemMessage orderItem in context.Message.OrderItems)
                {
                    Stock.API.Models.Entities.Stock stock = await (await _stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();

                    stock.Count -= orderItem.Count;

                    await _stockCollection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId, stock);
                }
                // payment işlemi

                StockReservedEvent stockReservedEvent = new StockReservedEvent()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice
                };

               ISendEndpoint sendEndpoint= await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
               await sendEndpoint.Send(stockReservedEvent);
                await Console.Out.WriteLineAsync("Stok işlemleri başarılı..");
            }
            else
            {
                //siparişin tutarsız/geçersiz olduğuna dair işlemler

                StockNotReservedEvent stockNotReservedEvent = new StockNotReservedEvent()
                {
                    OrderId = context.Message.OrderId,
                    BuyerId = context.Message.BuyerId,
                    Message = "Sipariş Tutarsız/Geçersiz"
                };
                await _publishEndpoint.Publish(stockNotReservedEvent);
                await Console.Out.WriteLineAsync("Stok işlemleri başarısız..");
            }

           // return Task.CompletedTask;
        }
    }
}
