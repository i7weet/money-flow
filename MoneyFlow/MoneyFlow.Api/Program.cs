using Microsoft.EntityFrameworkCore;
using MoneyFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MoneyFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
