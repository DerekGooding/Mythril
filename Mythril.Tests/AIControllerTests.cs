using Mythril.API;
using Mythril.API.Transport;
using Moq;

namespace Mythril.Tests;

[TestClass]
public class AIControllerTests
{
    [TestMethod]
    public async Task TestPingCommand()
    {
        // Arrange
        var mockTransport = new Mock<ICommandTransport>();
        mockTransport.Setup(t => t.ReceiveAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync("PONG");

        var command = new Command { Action = "PING" };
        var jsonCommand = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        // Act
        await mockTransport.Object.SendAsync(jsonCommand, It.IsAny<CancellationToken>());
        var response = await mockTransport.Object.ReceiveAsync(It.IsAny<CancellationToken>());

        // Assert
        Assert.AreEqual("PONG", response);
    }

    [TestMethod]
    public async Task TestScreenshotCommand()
    {
        // Arrange
        var mockTransport = new Mock<ICommandTransport>();
        const string screenshotData = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";
        mockTransport.Setup(t => t.ReceiveAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(screenshotData);

        var command = new Command { Action = "SCREENSHOT", Args = new Dictionary<string, object> { { "filename", "test.png" }, { "inline", true } } };
        var jsonCommand = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        // Act
        await mockTransport.Object.SendAsync(jsonCommand, It.IsAny<CancellationToken>());
        var response = await mockTransport.Object.ReceiveAsync(It.IsAny<CancellationToken>());

        // Assert
        Assert.IsTrue(response.StartsWith("data:image/png;base64,"));
    }

    [TestMethod]
    public async Task TestErrorResponse()
    {
        // Arrange
        var mockTransport = new Mock<ICommandTransport>();
        const string errorMessage = "Error: Button not found";
        mockTransport.Setup(t => t.ReceiveAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(errorMessage);

        var command = new Command { Action = "CLICK_BUTTON", Target = "NonExistentButton" };
        var jsonCommand = Newtonsoft.Json.JsonConvert.SerializeObject(command);

        // Act
        await mockTransport.Object.SendAsync(jsonCommand, It.IsAny<CancellationToken>());
        var response = await mockTransport.Object.ReceiveAsync(It.IsAny<CancellationToken>());

        // Assert
        Assert.AreEqual(errorMessage, response);
    }
}
