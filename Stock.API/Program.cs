using MassTransit;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddMassTransit(configurator =>
{
    //consumer tanýttýk
    configurator.AddConsumer<OrderCreatedEventConsumers>();

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);

        //rabbitmq içerisindeki hangi kuyruktan bu consumeri dinleme iþlemi yapýlacaðý
        _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e=>e.ConfigureConsumer<OrderCreatedEventConsumers>(context));
    });
});

builder.Services.AddSingleton<MongoDBService>();
//bir scope açýp iþimiz bittiðinde sil dedik
//using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();


//MongoDBService mongoDBService = scope.ServiceProvider.GetService<MongoDBService>();

//var collection = mongoDBService.GetCollection<Stock.API.Models.Entities.Stock>();
//await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 1000 });
//await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 5000 });
//await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 4000 });
//await collection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 5000 });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
