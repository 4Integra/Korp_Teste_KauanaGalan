using Billing.Api.Data;
using Billing.Api.Clients;
using Billing.Api.Services;
using Billing.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    var inventoryUrl =
        builder.Configuration["Services:InventoryUrl"]
        ?? throw new InvalidOperationException(
            "Inventory service URL is not configured.");

    client.BaseAddress = new Uri(inventoryUrl);
});

builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Billing API v1"
        );
    });
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
