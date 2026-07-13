using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NotificationService.Application.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Application.DTOs;
using Serilog;

namespace NotificationService.Infrastructure.Messaging;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly RabbitMQSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQConsumerService(IOptions<RabbitMQSettings> settings, IServiceProvider serviceProvider)
    {
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };

        int retryCount = 0;
        const int maxRetries = 5;

        while (true)
        {
            try
            {
                Log.Information("Connecting consumer to RabbitMQ at {Host}:{Port} (Attempt {Attempt}/{Max})...", 
                    _settings.HostName, _settings.Port, retryCount + 1, maxRetries);
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare Dead Letter Exchange and Queue
                _channel.ExchangeDeclare(_settings.DlxName, ExchangeType.Direct, durable: true, autoDelete: false);
                _channel.QueueDeclare(_settings.DlqName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_settings.DlqName, _settings.DlxName, _settings.DlqName);

                // Declare Main Exchange and Queue (linked to DLX)
                _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
                
                var arguments = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", _settings.DlxName },
                    { "x-dead-letter-routing-key", _settings.DlqName }
                };
                
                _channel.QueueDeclare(_settings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
                _channel.QueueBind(_settings.QueueName, _settings.ExchangeName, "request.created");
                _channel.QueueBind(_settings.QueueName, _settings.ExchangeName, "request.statuschanged");

                // Set prefetch count to 1 for fair dispatching
                _channel.BasicQos(0, 1, false);

                Log.Information("Successfully connected consumer to RabbitMQ and declared topology.");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                Log.Error(ex, "Failed to connect consumer to RabbitMQ.");
                if (retryCount >= maxRetries)
                {
                    Log.Warning("RabbitMQ is not available. Background consumer will wait for connection recovery.");
                    break;
                }
                Thread.Sleep(3000);
            }
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        if (_channel == null || _channel.IsClosed)
        {
            Log.Error("RabbitMQ channel is closed. Consumer cannot start.");
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            Log.Information("Received message on RabbitMQ queue {Queue} with routing key {RoutingKey}", 
                _settings.QueueName, routingKey);

            bool processed = false;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                if (routingKey == "request.created")
                {
                    var ev = JsonSerializer.Deserialize<RequestCreatedEvent>(message);
                    if (ev != null)
                    {
                        var text = $"Korisnik {ev.EmployeeId} je kreirao zahtev tipa {ev.RequestType} od {ev.StartDate:dd.MM.yyyy} do {ev.EndDate:dd.MM.yyyy}. Opis: {ev.Description}";
                        await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            RecipientId = ev.EmployeeId,
                            Message = text
                        }, stoppingToken);
                        processed = true;
                    }
                }
                else if (routingKey == "request.statuschanged")
                {
                    var ev = JsonSerializer.Deserialize<RequestStatusChangedEvent>(message);
                    if (ev != null)
                    {
                        var text = $"Status vašeg zahteva #{ev.RequestId} je promenjen u: {ev.NewStatus}. (Odobrio/odbio: {ev.ApproverRole ?? "System"})";
                        await notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            RecipientId = ev.EmployeeId,
                            Message = text
                        }, stoppingToken);
                        processed = true;
                    }
                }
                else
                {
                    Log.Warning("Unknown routing key received: {RoutingKey}", routingKey);
                    processed = true; // Mark as processed to ack it, or send to DLQ? Nack will dead-letter it.
                }

                if (processed)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    Log.Information("Successfully processed message and sent manual ACK.");
                }
                else
                {
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    Log.Warning("Could not deserialize message. Sent NACK (Dead Lettered).");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception thrown while processing message. Sent NACK (Dead Lettered). Message: {Message}", message);
                try
                {
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
                catch (Exception nackEx)
                {
                    Log.Error(nackEx, "Failed to send NACK for message.");
                }
            }
        };

        _channel.BasicConsume(queue: _settings.QueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Stopping RabbitMQ Background Consumer...");
        await base.StopAsync(cancellationToken);
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while closing RabbitMQ consumer.");
        }
    }
}
