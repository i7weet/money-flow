using MoneyFlow.Notifications.Worker;
using MoneyFlow.Notifications.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.HostName), "RabbitMq:HostName is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "RabbitMq:UserName is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.QueueName), "RabbitMq:Queue is required")
    .Validate(options => options.Port is >= 1 and <= 65535, "RabbitMq:Port not valid")
    .ValidateOnStart();

var app = builder.Build();
app.Run();
