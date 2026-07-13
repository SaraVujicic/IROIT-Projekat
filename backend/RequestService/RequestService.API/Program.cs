using RequestService.Application;
using RequestService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RequestService.API.Middleware;
using RequestService.Infrastructure.Data;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "RequestService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("RequestService")
        .SetResourceBuilder(OpenTelemetry.Resources.ResourceBuilder.CreateDefault().AddService("RequestService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://jaeger:4317")));

// Add Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Register CorrelationIdDelegatingHandler
builder.Services.AddTransient<RequestService.API.Services.CorrelationIdDelegatingHandler>();

// Register typed HTTP Client with Delegating Handler
builder.Services.AddHttpClient<RequestService.Application.Services.IEmployeeServiceClient, RequestService.Application.Services.EmployeeServiceClient>(client => 
{
    client.BaseAddress = new Uri(builder.Configuration["EmployeeService:BaseUrl"] ?? "http://employeeservice:8080");
}).AddHttpMessageHandler<RequestService.API.Services.CorrelationIdDelegatingHandler>();

// Register RabbitMQ Settings & Message Bus
builder.Services.Configure<RequestService.Infrastructure.Messaging.RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RequestService.Application.Interfaces.IMessageBus, RequestService.Infrastructure.Messaging.RabbitMQMessageBus>();

var app = builder.Build();

app.UseMiddleware<RequestService.API.Middlewares.CorrelationIdMiddleware>();
app.UseHttpMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapMetrics();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var _db = scope.ServiceProvider.GetRequiredService<RequestDbContext>();
    _db.Database.Migrate();
}

app.Run();

// For Integration Tests
public partial class Program { }
