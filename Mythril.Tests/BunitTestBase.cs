using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mythril.Blazor.Services;
using Mythril.Data;

namespace Mythril.Tests;

public abstract class BunitTestBase : TestContextWrapper
{
    protected ResourceManager ResourceManager { get; private set; } = null!;
    protected JunctionManager JunctionManager { get; private set; } = null!;
    protected InventoryManager InventoryManager { get; private set; } = null!;
    protected DragDropService DragDropService { get; private set; } = null!;
    protected Stats Stats { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        TestContext = new Bunit.TestContext();
        
        TestContentLoader.Load();
        
        InventoryManager = new InventoryManager();
        Stats = ContentHost.GetContent<Stats>();
        var statAugments = ContentHost.GetContent<StatAugments>();
        var cadences = ContentHost.GetContent<Cadences>();
        
        JunctionManager = new JunctionManager(InventoryManager, statAugments, cadences);
        JunctionManager.Initialize();

        ResourceManager = new ResourceManager(
            ContentHost.GetContent<Items>(),
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<QuestToCadenceUnlocks>(),
            ContentHost.GetContent<QuestDetails>(),
            cadences,
            ContentHost.GetContent<Locations>(),
            JunctionManager,
            InventoryManager
        );
        ResourceManager.Initialize();
        
        DragDropService = new DragDropService();

        TestContext.Services.AddSingleton(ResourceManager);
        TestContext.Services.AddSingleton(JunctionManager);
        TestContext.Services.AddSingleton(InventoryManager);
        TestContext.Services.AddSingleton(DragDropService);
        TestContext.Services.AddSingleton(Stats);
        TestContext.Services.AddSingleton(ContentHost.GetContent<Items>());
        TestContext.Services.AddSingleton(new Mock<IJSRuntime>().Object);
    }

    [TestCleanup]
    public void TearDown() => TestContext?.Dispose();
}
