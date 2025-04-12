using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration from ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Register controller services
builder.Services.AddControllers();

// Register Ocelot services
builder.Services.AddOcelot(builder.Configuration);

// Add Swagger services for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger middleware in Development environment (or any environment as needed)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map controller endpoints (this exposes your GatewayController endpoints, etc.)
app.MapControllers();

// Use Ocelot middleware to route requests based on configuration
await app.UseOcelot();

app.Run();
