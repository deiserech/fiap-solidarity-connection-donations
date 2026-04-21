using Azure.Messaging.ServiceBus;
using SolidarityConnection.Donations.Api.BackgroundServices.Donations;
using SolidarityConnection.Donations.Api.Extensions;
using SolidarityConnection.Donations.Api.Middlewares;
using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Application.Interfaces.Repositories;
using SolidarityConnection.Donations.Application.Interfaces.Services;
using SolidarityConnection.Donations.Application.Services;
using SolidarityConnection.Donations.Infrastructure.Data;
using SolidarityConnection.Donations.Infrastructure.Messaging;
using SolidarityConnection.Donations.Infrastructure.Repositories;
using SolidarityConnection.Donations.Infrastructure.ServiceBus;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.OpenTelemetry(configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? ""));

var sbConnectionString = configuration["ServiceBus:ConnectionString"] ?? "";

builder.Services.AddSingleton(new ServiceBusClient(sbConnectionString));
builder.Services.AddSingleton<IServiceBusClientWrapper, ServiceBusClientWrapper>();
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();

builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<IDonationService, DonationService>();

builder.Services.AddScoped<IDonationRequestedMessageHandler, DonationRequestedMessageHandler>();

builder.Services.AddScoped<IDonationProcessedEventPublisher, DonationProcessedEventPublisher>();

builder.Services.AddHostedService<DonationRequestedConsumer>();

var app = builder.Build();

app.UseMiddleware<TracingEnrichmentMiddleware>();

app.MapHealthChecks("/health");

app.Run();
