using Microsoft.EntityFrameworkCore;
using RetailOps.Infrastructure.Persistence;
using RetailOps.Application;
using RetailOps.Application.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddSingleton<RetailOps.Infrastructure.Persistence.Interceptors.AuditInterceptor>();

builder.Services.AddDbContext<RetailOpsDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<RetailOps.Infrastructure.Persistence.Interceptors.AuditInterceptor>());
});

builder.Services.AddScoped<IRetailOpsDbContext>(provider => provider.GetRequiredService<RetailOpsDbContext>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RetailOpsDbContext>();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RetailOps.Api.Middleware.GlobalExceptionHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("admin", "manager"));
});

builder.Services.AddScoped<RetailOps.Api.Filters.IdempotencyFilter>();
builder.Services.AddSingleton<RetailOps.Infrastructure.Persistence.Interceptors.AuditInterceptor>();

builder.Services.AddHostedService<RetailOps.Infrastructure.BackgroundJobs.OutboxWorker>();

var app = builder.Build();

app.UseExceptionHandler(); // Enable global exception handling

app.UseMiddleware<RetailOps.Api.Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<RetailOps.Api.Middleware.RoleMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
