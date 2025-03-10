using BidManagement.Context;
using BidManagement.Repositories;
using BidManagement.Services;
using BidManagement.WinningBidStrategy;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<BidDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

// Register repositories
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IDecisionRepository, DecisionRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();

builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddSingleton<IConnection>(serviceProvider =>
{
    var factory = new ConnectionFactory() { HostName = "localhost" };

    Task<IConnection> connectionTask = factory.CreateConnectionAsync();
    connectionTask.Wait(); 

    return connectionTask.Result;
});

builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddHostedService<ProcessService>();


builder.Services.AddTransient<IWinningBidStrategy, LoyalCustomerWinningBedStrategy>();
builder.Services.AddTransient<IWinningBidStrategy, GenericWinningBidStrategy>();
builder.Services.AddTransient<IWinningBidStrategy, FirstInWinningBidStrategy>();
builder.Services.AddSingleton<StrategySelector>();
builder.Services.AddSingleton<IEmailSenderService, EmailSenderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
