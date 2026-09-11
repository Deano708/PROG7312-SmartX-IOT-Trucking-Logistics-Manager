using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Client;
using SmartX.Client.Services;
using SmartX.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Register HttpClient pointing to the .NET Web API base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7200/") }); // API Server URL

// Register API Client and local services
builder.Services.AddScoped<SmartXApiClient>();
builder.Services.AddSingleton<KznDataSeeder>();

await builder.Build().RunAsync();