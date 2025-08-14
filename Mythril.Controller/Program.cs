using Mythril.API;
using Mythril.API.Transport;
using Mythril.Controller.Transport;
using Newtonsoft.Json;

namespace Mythril.Controller;

static class Program
{
    static async Task Main()
    {
        Console.WriteLine("Mythril AI Controller Test Script Started.");

        // 1. Load configuration (still useful for game executable path)
        var config = ConfigLoader.LoadConfig();

        // 2. Hardcode StdIoTransport for testing
        ICommandTransport transport = new StdIoTransport();
        Console.WriteLine($"Using Transport: {transport.GetType().Name}");

        // 3. Launch the game
        var gameExecutablePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Mythril", "bin", "Debug", "net9.0", "Mythril.exe");
        gameExecutablePath = Path.GetFullPath(gameExecutablePath);

        var processManager = new ProcessManager(gameExecutablePath);

        try
        {
            processManager.StartGame();

            // Give the game some time to start up and initialize
            await Task.Delay(TimeSpan.FromSeconds(5));

            // 4. Define a sequence of commands
            var commands = new Command[]
            {
                new() { Action = "PING" },
                new() { Action = "WAIT", Args = new Dictionary<string, object> { { "seconds", 2.0 } } },
                new() { Action = "CLICK_BUTTON", Target = "Settings" },
                new() { Action = "WAIT", Args = new Dictionary<string, object> { { "seconds", 1.0 } } },
                new() { Action = "SCREENSHOT", Args = new Dictionary<string, object> { { "filename", "screenshot1.png" }, { "inline", true } } },
                new() { Action = "WAIT", Args = new Dictionary<string, object> { { "seconds", 1.0 } } },
                new() { Action = "CLICK_BUTTON", Target = "Close" }, // Assuming Settings dialog has a Close button
                new() { Action = "WAIT", Args = new Dictionary<string, object> { { "seconds", 1.0 } } },
                new() { Action = "SCREENSHOT", Args = new Dictionary<string, object> { { "filename", "screenshot2.png" }, { "inline", true } } },
                new() { Action = "EXIT" }
            };

            // 5. Send commands and verify responses
            foreach (var cmd in commands)
            {
                var jsonCommand = JsonConvert.SerializeObject(cmd);
                Console.WriteLine($"Sending command: {jsonCommand}");
                await transport.SendAsync(jsonCommand);

                // Receive response from game
                var gameResponse = await transport.ReceiveAsync();
                Console.WriteLine($"Game response: {gameResponse}");

                // Basic verification for screenshot
                if (cmd.Action == "SCREENSHOT" && gameResponse.StartsWith("data:image/png;base64,"))
                {
                    Console.WriteLine("Received inline base64 screenshot.");
                    // In a real scenario, you would decode and save/process the image data
                    var base64Image = gameResponse.Substring("data:image/png;base64,".Length);
                    var imageData = Convert.FromBase64String(base64Image);
                    Console.WriteLine($"Screenshot data length: {imageData.Length} bytes.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            await processManager.StopGameAsync();
            Console.WriteLine("Mythril AI Controller Test Script Finished.");
        }
    }
}