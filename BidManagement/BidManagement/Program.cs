using BidManagement.Context;
using BidManagement.Repositories;
using BidManagement.Services;
using BidManagement.WinningBidStrategy;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("D:\\Log/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddDbContext<BidDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

// Register repositories
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IDecisionRepository, DecisionRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();

builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddSingleton<IConnection>(serviceProvider => null!); 
builder.Services.AddSingleton<IChannel>(serviceProvider => null!); 
builder.Services.AddSingleton<IQueueService, QueueService>();



builder.Services.AddHostedService<RabbitMqInitializationService>();

builder.Services.AddHostedService<ProcessService>();


// Register Strategy Implementations
builder.Services.AddTransient<LoyalCustomerWinningBedStrategy>();
builder.Services.AddTransient<FirstInWinningBidStrategy>();
builder.Services.AddTransient<GenericWinningBidStrategy>();

// Register IWinningBidStrategy implementations with concrete strategies
builder.Services.AddTransient<IWinningBidStrategy>(provider => provider.GetRequiredService<LoyalCustomerWinningBedStrategy>());
builder.Services.AddTransient<IWinningBidStrategy>(provider => provider.GetRequiredService<FirstInWinningBidStrategy>());
builder.Services.AddTransient<IWinningBidStrategy>(provider => provider.GetRequiredService<GenericWinningBidStrategy>());

// Register StrategySelector with concrete strategies
builder.Services.AddScoped<StrategySelector>(serviceProvider =>
{
    var loyalCustomerStrategy = serviceProvider.GetRequiredService<LoyalCustomerWinningBedStrategy>();
    var firstInStrategy = serviceProvider.GetRequiredService<FirstInWinningBidStrategy>();
    var genericStrategy = serviceProvider.GetRequiredService<GenericWinningBidStrategy>();

    var strategies = new Dictionary<string, IWinningBidStrategy>
    {
        { "BMW", loyalCustomerStrategy },
        { "Mercedes", loyalCustomerStrategy },
        { "Peugeot", firstInStrategy },
        { "Generic", genericStrategy }
    };

    return new StrategySelector(strategies);
});

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
