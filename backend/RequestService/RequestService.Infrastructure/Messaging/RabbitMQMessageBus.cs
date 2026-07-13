using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RequestService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace RequestService.Infrastructure.Messaging;

public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMQMessageBus(IOptions<RabbitMQSettings> settings)
    {
        _settings = settings.Value;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        lock (_lock)
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
                    Log.Information("Connecting to RabbitMQ at {Host}:{Port} (Attempt {Attempt}/{Max})...", 
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

                    // Enable Publisher Confirms
                    _channel.ConfirmSelect();

                    Log.Information("Successfully connected to RabbitMQ and declared topology.");
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Log.Error(ex, "Failed to connect to RabbitMQ.");
                    if (retryCount >= maxRetries)
                    {
                        Log.Warning("RabbitMQ is not available. Continuing startup; connection will be recovered automatically when RabbitMQ starts.");
                        break;
                    }
                    Thread.Sleep(3000);
                }
            }
        }
    }

    public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                if (_channel == null || _channel.IsClosed)
                {
                    Log.Warning("RabbitMQ channel is not open. Reinitializing connection...");
                    InitializeRabbitMQ();
                }

                if (_channel != null && !_channel.IsClosed)
                {
                    var json = JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true; // Delivery Mode 2 (Persistent)
                    properties.ContentType = "application/json";

                    lock (_lock)
                    {
                        _channel.BasicPublish(
                            exchange: _settings.ExchangeName,
                            routingKey: routingKey,
                            basicProperties: properties,
                            body: body);

                        // Wait for publisher confirm
                        _channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
                    }
                    
                    Log.Information("Published message of type {MessageType} to exchange {Exchange} with routing key {RoutingKey}", 
                        typeof(T).Name, _settings.ExchangeName, routingKey);
                }
                else
                {
                    Log.Error("Could not publish message: RabbitMQ channel is unavailable.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while publishing message to RabbitMQ.");
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error closing RabbitMQ connection.");
        }
    }
}
