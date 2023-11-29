using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            //Ödeme işlemleri

            if(true)
            {
                PaymentCompletedEvent paymentCompleted = new PaymentCompletedEvent()
                {
                    OrderId=context.Message.OrderId
                };
                _publishEndpoint.Publish(paymentCompleted);

                Console.WriteLine("Ödeme başarılı..");
            }
            else
            {
                //ödemede sıkıntı olunca
                PaymentFailedEvent paymentFailed = new PaymentFailedEvent()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Bakiye yetersiz."
                };
                _publishEndpoint.Publish(paymentFailed);

                Console.WriteLine("Ödeme başarısız..");
            }
            return Task.CompletedTask;
        }
    }
}
