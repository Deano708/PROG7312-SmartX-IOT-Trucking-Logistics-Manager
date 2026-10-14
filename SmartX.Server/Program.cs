using SmartX.Shared.Services;
using SmartX.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register shared logic services for API consumption
builder.Services.AddSingleton<KznDataSeeder>();
builder.Services.AddSingleton<TelemetryBatchProcessor>();
builder.Services.AddSingleton<TelemetryHistoryStore>();
builder.Services.AddSingleton<EmergencyAlertQueue>();
builder.Services.AddSingleton<DisconnectedSensorTracker>();

// Configure CORS for Blazor WASM frontend client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll"); // Must be before MapControllers
app.UseAuthorization();
app.MapControllers();

app.Run();