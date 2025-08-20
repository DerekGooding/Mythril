using Mythril.API;
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

        // 2. Launch the game
        var gameExecutablePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "publish", "Mythril");
        gameExecutablePath = Path.GetFullPath(gameExecutablePath);

        var processManager = new ProcessManager(gameExecutablePath);

        try
        {
            processManager.StartGame();

            // 3. Create transport with game's I/O streams
            var transport = new StdIoTransport(processManager.GetStandardOutput()!, processManager.GetStandardInput()!);
            Console.WriteLine($"Using Transport: {transport.GetType().Name}");

            // Give the game some time to start up and initialize
            await Task.Delay(TimeSpan.FromSeconds(5));

            // 4. Define a sequence of commands
            var commands = new Command[]
            {
                new() { Action = "CLICK_BUTTON", Target = "Test Combat" },
                new() { Action = "WAIT", Args = new Dictionary<string, object> { { "seconds", 2.0 } } },
                new() { Action = "CLICK_BUTTON", Target = "Attack" },
                new() { Action = "WAIT", Args = new Dictionary<string, object> { { "seconds", 2.0 } } },
                new() { Action = "SCREENSHOT", Args = new Dictionary<string, object> { { "filename", "screenshot1.png" }, { "inline", true } } },
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
                    Console.WriteLine("JULES_SCREENSHOT_START");
                    Console.WriteLine(gameResponse);
                    Console.WriteLine("JULES_SCREENSHOT_END");
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