using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using FluentValidation;
using LeaveBalanceService.Application.Interfaces;
using LeaveBalanceService.Application.Services;
using LeaveBalanceService.Application.DTOs;
using LeaveBalanceService.Application.Validators;
using LeaveBalanceService.Domain.Repositories;
using LeaveBalanceService.Infrastructure.Data;
using LeaveBalanceService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LeaveBalanceService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// Add services to the container.
builder.Services.AddControllers();

// Configure FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateLeaveBalanceDtoValidator>();
builder.Services.AddFluentValidationAutoValidation(); // From SharpGrip.FluentValidation.AutoValidation.Mvc

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("LeaveBalanceService")
        .SetResourceBuilder(OpenTelemetry.Resources.ResourceBuilder.CreateDefault().AddService("LeaveBalanceService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://jaeger:4317")));

// Infrastructure
builder.Services.AddDbContext<LeaveBalanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();

// Application
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService.Application.Services.LeaveBalanceService>();

var app = builder.Build();

app.UseMiddleware<LeaveBalanceService.API.Middlewares.CorrelationIdMiddleware>();
app.UseHttpMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Global Exception Handler Middleware
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (System.Exception ex)
    {
        Log.Error(ex, "An unhandled exception occurred.");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
    }
});

app.MapMetrics();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var _db = scope.ServiceProvider.GetRequiredService<LeaveBalanceDbContext>();
    _db.Database.Migrate();
}

app.Run();

// Needed for Integration Testing setup
public partial class Program { }
