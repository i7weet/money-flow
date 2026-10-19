using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MoneyFlow.Api.Endpoints;
using MoneyFlow.Api.ErrorHandling;
using MoneyFlow.Application.Accounts;
using MoneyFlow.Application.Accounts.CreateAccount;
using MoneyFlow.Application.Accounts.GetAccount;
using MoneyFlow.Application.Messaging;
using MoneyFlow.Infrastructure.Messaging;
using MoneyFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<MoneyFlowDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IAccountStore, AccountStore>();

builder.Services.AddScoped<CreateAccountService>();
builder.Services.AddScoped<GetAccountService>();

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

builder.Services.AddHealthChecks().AddDbContextCheck<MoneyFlowDbContext>(name: "postgresql", tags: ["ready"]);
builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.HostName), "RabbitMq:HostName is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "RabbitMq:UserName is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.QueueName), "RabbitMq:Queue is required")
    .Validate(options => options.Port is >= 1 and <= 65535, "RabbitMq:Port not valid")
    .ValidateOnStart();
builder.Services.AddSingleton<IAccountCreatedPublisher, RabbitMqAccountCreatedPublisher>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapGet("/", () => "Hello World!");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapAccountEndpoints();

app.Run();

public partial class Program { }
