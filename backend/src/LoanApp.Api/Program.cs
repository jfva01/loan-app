using LoanApp.Application.Abstractions;
using LoanApp.Application.Rules;
using LoanApp.Application.Services;
using LoanApp.Domain.Rules;
using LoanApp.Infrastructure.External;
using LoanApp.Infrastructure.Persistence;
using LoanApp.Infrastructure.Persistence.Repositories;
using LoanApp.Infrastructure.Rules;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Registramos el DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<IDenyRule, NyStateDenyRule>();
builder.Services.AddScoped<IDenyRule, BlacklistedSsnDenyRule>();
builder.Services.AddScoped<IRuleEngine, RuleEngine>();
builder.Services.AddScoped<IBlacklistProvider, AppSettingsBlacklistProvider>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient<IExternalLoanService, HttpExternalLoanService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5262/");
});

builder.Services.AddScoped<LoanApplicationService>();
builder.Services.AddHostedService<LoanApp.Infrastructure.BackgroundJobs.OutboxProcessor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5298")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();

public partial class Program { }