using MedicationService.Data;
using MedicationService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Register MedicationDbContext with SQL Server
builder.Services.AddDbContext<MedicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services with HttpClient
builder.Services.AddHttpClient<IExternalMedicationApiService, ExternalMedicationApiService>();
builder.Services.AddHttpClient<IAiConflictCheckService, AiConflictCheckService>();

// Register MemoryCache
builder.Services.AddMemoryCache();

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting(); // Ensure routing is enabled
app.MapControllers();

app.Run();