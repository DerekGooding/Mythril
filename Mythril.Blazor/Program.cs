using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Mythril.Blazor;
using Mythril.Blazor.Services;
using Mythril.Data;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DragDropService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<VersionService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<FeedbackService>();
builder.Services.AddSingleton<SnackbarService>();

// Register the logger provider correctly within the service collection
builder.Logging.Services.AddSingleton<ILoggerProvider, FeedbackLoggerProvider>();

// Register Content (Singletons)
builder.Services.AddSingleton(ContentHost.GetContent<Items>());
builder.Services.AddSingleton(ContentHost.GetContent<Quests>());
builder.Services.AddSingleton(ContentHost.GetContent<Cadences>());
builder.Services.AddSingleton(ContentHost.GetContent<Locations>());
builder.Services.AddSingleton(ContentHost.GetContent<QuestDetails>());
builder.Services.AddSingleton(ContentHost.GetContent<QuestUnlocks>());
builder.Services.AddSingleton(ContentHost.GetContent<CadenceAbilities>());
builder.Services.AddSingleton(ContentHost.GetContent<QuestToCadenceUnlocks>());
builder.Services.AddSingleton(ContentHost.GetContent<Stats>());
builder.Services.AddSingleton(ContentHost.GetContent<StatAugments>());
builder.Services.AddSingleton(ContentHost.GetContent<AbilityAugments>());
builder.Services.AddSingleton(ContentHost.GetContent<ItemRefinements>());

// Register Loader & Engine
builder.Services.AddScoped<ContentLoader>();
builder.Services.AddSingleton<InventoryManager>();
builder.Services.AddSingleton<JunctionManager>();
builder.Services.AddSingleton<ResourceManager>();

// Register Persistence
builder.Services.AddScoped<PersistenceService>();

await builder.Build().RunAsync();
