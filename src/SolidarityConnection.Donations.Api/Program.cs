using Azure.Messaging.ServiceBus;
using SolidarityConnection.Donations.Api.BackgroundServices.Campaigns;
using SolidarityConnection.Donations.Api.BackgroundServices.Donations;
using SolidarityConnection.Donations.Api.BackgroundServices.Donors;
using SolidarityConnection.Donations.Api.Extensions;
using SolidarityConnection.Donations.Api.Middlewares;
using SolidarityConnection.Donations.Application.Interfaces.Publishers;
using SolidarityConnection.Donations.Application.Interfaces.Repositories;
using SolidarityConnection.Donations.Application.Interfaces.Services;
using SolidarityConnection.Donations.Application.Services;
using SolidarityConnection.Donations.Domain.Interfaces.Repositories;
using SolidarityConnection.Donations.Infrastructure.Data;
using SolidarityConnection.Donations.Infrastructure.Messaging;
using SolidarityConnection.Donations.Infrastructure.Repositories;
using SolidarityConnection.Donations.Infrastructure.ServiceBus;
using SolidarityConnection.Donations.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddSwagger();

builder.Services.OpenTelemetry();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? ""));

var sbConnectionString = configuration["ServiceBus:ConnectionString"] ?? "";
builder.Services.Configure<ServiceBusOptions>(opts => { opts.ConnectionString = sbConnectionString; });

builder.Services.AddSingleton(new ServiceBusClient(sbConnectionString));
builder.Services.AddSingleton<IServiceBusClientWrapper, ServiceBusClientWrapper>();
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();

builder.Services.AddScoped<ICampaignReferenceRepository, CampaignReferenceRepository>();
builder.Services.AddScoped<ICampaignReferenceService, CampaignReferenceService>();
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IDonorReferenceRepository, DonorReferenceRepository>();
builder.Services.AddScoped<IDonorReferenceService, DonorReferenceService>();

builder.Services.AddScoped<IDonationRequestedMessageHandler, DonationRequestedMessageHandler>();
builder.Services.AddScoped<ICampaignUpsertedMessageHandler, CampaignUpsertedMessageHandler>();
builder.Services.AddScoped<IDonorUpsertedMessageHandler, DonorUpsertedMessageHandler>();

builder.Services.AddScoped<IDonationRequestedEventPublisher, DonationRequestedEventPublisher>();
builder.Services.AddScoped<IDonationProcessedEventPublisher, DonationProcessedEventPublisher>();

builder.Services.AddHostedService<CampaignUpsertedConsumer>();
builder.Services.AddHostedService<DonationRequestedConsumer>();
builder.Services.AddHostedService<DonorUpsertedConsumer>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TracingEnrichmentMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
