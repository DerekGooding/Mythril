using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mythril.Blazor;
using Mythril.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DragDropService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddSingleton<SnackbarService>();
builder.Services.AddSingleton<Mythril.Data.ResourceManager>();


await builder.Build().RunAsync();
