namespace NotificationService.Infrastructure.Messaging;

public class RabbitMQSettings
{
    public string HostName { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "request-events-exchange";
    public string QueueName { get; set; } = "request-events-queue";
    public string DlxName { get; set; } = "request-events-dlx";
    public string DlqName { get; set; } = "request-events-dlq";
}
