using System.Net;
using System.Text;
using Mythril.Data;
using Moq;
using Moq.Protected;

namespace Mythril.Tests;

[TestClass]
public class ContentLoaderTests
{
    [TestMethod]
    public async Task ContentLoader_LoadAllAsync_Works()
    {
        // 1. Setup Mock Http
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        string dataDir = GetTestDataDir();

        // Helper to setup mock response for a file
        void SetupMock(string url, string fileName)
        {
            var content = File.ReadAllText(Path.Combine(dataDir, fileName));
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(url)),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(content, Encoding.UTF8, "application/json"),
               });
        }

        SetupMock("data/items.json", "items.json");
        SetupMock("data/stats.json", "stats.json");
        SetupMock("data/cadence_abilities.json", "cadence_abilities.json");
        SetupMock("data/quests.json", "quests.json");
        SetupMock("data/locations.json", "locations.json");
        SetupMock("data/cadences.json", "cadences.json");
        SetupMock("data/quest_details.json", "quest_details.json");
        SetupMock("data/quest_unlocks.json", "quest_unlocks.json");
        SetupMock("data/refinements.json", "refinements.json");
        SetupMock("data/quest_cadence_unlocks.json", "quest_cadence_unlocks.json");
        SetupMock("data/stat_augments.json", "stat_augments.json");

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com/") };

        // 2. Instantiate ContentLoader
        var loader = new ContentLoader(
            httpClient,
            ContentHost.GetContent<Items>(),
            ContentHost.GetContent<Stats>(),
            ContentHost.GetContent<CadenceAbilities>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<Locations>(),
            ContentHost.GetContent<Cadences>(),
            ContentHost.GetContent<QuestDetails>(),
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<ItemRefinements>(),
            ContentHost.GetContent<StatAugments>(),
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        // 3. Act
        await loader.LoadAllAsync();

        // 4. Assert
        Assert.IsTrue(ContentHost.GetContent<Items>().All.Any());
        Assert.IsTrue(ContentHost.GetContent<Quests>().All.Any());
    }

    private string GetTestDataDir()
    {
        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        string? rootDir = currentDir;
        while (rootDir != null && !File.Exists(Path.Combine(rootDir, "Mythril.sln")))
        {
            rootDir = Path.GetDirectoryName(rootDir);
        }
        if (rootDir == null) throw new Exception("Could not find solution root.");
        return Path.Combine(rootDir, "Mythril.Blazor/wwwroot/data");
    }
}
