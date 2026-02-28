using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mythril.Blazor;
using Mythril.Blazor.Services;
using Mythril.Data;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DragDropService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddSingleton<SnackbarService>();
builder.Services.AddSingleton<Mythril.Data.ResourceManager>();

// Register Content
builder.Services.AddSingleton(ContentHost.GetContent<Items>());
builder.Services.AddSingleton(ContentHost.GetContent<Quests>());
builder.Services.AddSingleton(ContentHost.GetContent<Cadences>());
builder.Services.AddSingleton(ContentHost.GetContent<QuestDetails>());
builder.Services.AddSingleton(ContentHost.GetContent<CadenceAbilities>());

// Register Persistence
builder.Services.AddScoped<PersistenceService>();

await builder.Build().RunAsync();
