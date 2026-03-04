using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Mythril.Blazor.Services;
using Mythril.Data;
using System.Net.Http;

namespace Mythril.Tests;

public abstract class BunitTestBase : TestContextWrapper
{
    protected ResourceManager ResourceManager { get; private set; } = null!;
    protected JunctionManager JunctionManager { get; private set; } = null!;
    protected InventoryManager InventoryManager { get; private set; } = null!;
    protected DragDropService DragDropService { get; private set; } = null!;
    protected Stats Stats { get; private set; } = null!;
    protected Mock<IJSRuntime> JSRuntimeMock { get; private set; } = null!;
    protected SnackbarService SnackbarService { get; private set; } = null!;
    protected AuthService AuthService { get; private set; } = null!;
    protected Mock<ContentLoader> ContentLoaderMock { get; private set; } = null!;

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
            InventoryManager,
            ContentHost.GetContent<ItemRefinements>()
        );
        ResourceManager.Initialize();
        
        DragDropService = new DragDropService();
        JSRuntimeMock = new Mock<IJSRuntime>();
        SnackbarService = new SnackbarService();
        AuthService = new AuthService(JSRuntimeMock.Object);
        var httpClient = new HttpClient();
        
        ContentLoaderMock = new Mock<ContentLoader>(
            httpClient,
            ContentHost.GetContent<Items>(),
            ContentHost.GetContent<Stats>(),
            ContentHost.GetContent<CadenceAbilities>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<Locations>(),
            cadences,
            ContentHost.GetContent<QuestDetails>(),
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<ItemRefinements>(),
            statAugments,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        TestContext.Services.AddSingleton(ResourceManager);
        TestContext.Services.AddSingleton(JunctionManager);
        TestContext.Services.AddSingleton(InventoryManager);
        TestContext.Services.AddSingleton(DragDropService);
        TestContext.Services.AddSingleton(Stats);
        TestContext.Services.AddSingleton(ContentHost.GetContent<Items>());
        TestContext.Services.AddSingleton(ContentHost.GetContent<Cadences>());
        TestContext.Services.AddSingleton(ContentHost.GetContent<Locations>());
        TestContext.Services.AddSingleton(ContentHost.GetContent<ItemRefinements>());
        TestContext.Services.AddSingleton(JSRuntimeMock.Object);
        TestContext.Services.AddSingleton(SnackbarService);
        TestContext.Services.AddSingleton(AuthService);
        TestContext.Services.AddSingleton(ContentLoaderMock.Object);
        TestContext.Services.AddSingleton(new InventoryService());
        TestContext.Services.AddSingleton(new ThemeService(JSRuntimeMock.Object, new Mock<ILogger<ThemeService>>().Object));
        
        TestContext.Services.AddSingleton(new Mock<PersistenceService>(
            JSRuntimeMock.Object,
            ResourceManager,
            JunctionManager,
            ContentHost.GetContent<Items>(),
            cadences,
            ContentHost.GetContent<Quests>(),
            Stats,
            ContentHost.GetContent<ItemRefinements>()
        ).Object);

        TestContext.Services.AddSingleton(new Mock<FeedbackService>(JSRuntimeMock.Object, AuthService, httpClient).Object);
        TestContext.Services.AddSingleton(new Mock<VersionService>(httpClient).Object);
    }

    [TestCleanup]
    public void TearDown() => TestContext?.Dispose();
}
