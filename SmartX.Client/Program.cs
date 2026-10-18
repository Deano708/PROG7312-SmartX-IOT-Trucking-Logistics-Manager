using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Client;
using SmartX.Client.Services;
using SmartX.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. HttpClient pointing to SmartX.Server HTTPS URL
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7044/") });
builder.Services.AddSingleton<TruckSensorStateService>();

// 2. Register required services for Blazor components
builder.Services.AddScoped<SmartXApiClient>();
builder.Services.AddScoped<CommandHistoryService>();
builder.Services.AddSingleton<KznDataSeeder>();
builder.Services.AddSingleton<TelemetryAggregator>();

await builder.Build().RunAsync();