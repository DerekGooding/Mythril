using Moq;
using Moq.Protected;
using Mythril.Data;
using System.Net;
using System.Text;

namespace Mythril.Tests;

[TestClass]
public class ContentLoaderTests
{
    [TestMethod]
    public async Task ContentLoader_LoadAllAsync_Works()
    {
        // 1. Setup Mock Http
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var dataDir = GetTestDataDir();

        // Helper to setup mock response for a file
        void SetupMock(string url, string fileName)
        {
            var path = Path.Combine(dataDir, fileName);
            if (!File.Exists(path)) throw new FileNotFoundException($"Test file not found: {path}");

            var content = File.ReadAllText(path);
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

        // Only these two files are loaded by the new ContentLoader
        SetupMock("data/content_graph.json", "content_graph.json");
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
            ContentHost.GetContent<AbilityAugments>(),
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        // 3. Act
        await loader.LoadAllAsync();

        // 4. Assert
        Assert.IsNotEmpty(ContentHost.GetContent<Items>().All);
        Assert.IsNotEmpty(ContentHost.GetContent<Quests>().All);
    }

    private string GetTestDataDir()
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var rootDir = currentDir;
        while (rootDir != null && !File.Exists(Path.Combine(rootDir, "Mythril.sln")))
        {
            rootDir = Path.GetDirectoryName(rootDir);
        }
        if (rootDir == null) throw new Exception("Could not find solution root.");
        return Path.Combine(rootDir, "Mythril.Blazor/wwwroot/data");
    }
}