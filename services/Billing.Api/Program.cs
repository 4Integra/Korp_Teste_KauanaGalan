using Billing.Api.Data;
using Billing.Api.Clients;
using Billing.Api.Services;
using Billing.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("Frontend");

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

app.UseAuthorization();

app.MapControllers();

app.Run();
