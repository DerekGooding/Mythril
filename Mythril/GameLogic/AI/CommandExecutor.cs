using Myra.Graphics2D.UI;

namespace Mythril.GameLogic.AI;

public class CommandExecutor(Game1 game, Desktop desktop, ScreenshotUtility screenshotUtility)
{
    private readonly Game1 _game = game;
    private readonly Desktop _desktop = desktop;
    private readonly ScreenshotUtility _screenshotUtility = screenshotUtility;

    public async Task ExecuteCommand(Command command)
    {
        switch (command.Action.ToUpperInvariant())
        {
            case "CLICK_BUTTON":
                HandleClickButton(command);
                break;
            case "CLICK_COORDS":
                HandleClickCoords(command);
                break;
            case "WAIT":
                await HandleWait(command);
                break;
            case "SCREENSHOT":
                HandleScreenshot(command);
                break;
            case "PING":
                HandlePing();
                break;
            case "EXIT":
                HandleExit();
                break;
            default:
                Console.WriteLine($"Unknown command: {command.Action}");
                break;
        }
    }

    private void HandleClickButton(Command command)
    {
        if (command.Target == null)
        {
            Console.WriteLine("CLICK_BUTTON command requires a 'target' (button text).");
            return;
        }

        // Find the button by its text and simulate a click
        // This is a simplified example and might need more robust UI traversal
        var button = _desktop.Root.FindWidgetById(command.Target) as Button;
        if (button == null)
        {
            // Try to find by label text
            foreach (var widget in _desktop.Root.GetChildren())
            {
                if (widget is Button btn && btn.Content is Label label && label.Text == command.Target)
                {
                    button = btn;
                    break;
                }
            }
        }

        if (button != null)
        {
            Console.WriteLine($"Clicking button: {command.Target}");
            button.DoClick();
        }
        else
        {
            Console.WriteLine($"Button not found: {command.Target}");
        }
    }

    private void HandleClickCoords(Command command)
    {
        if (!command.Args.TryGetValue("x", out var xObj) || !command.Args.TryGetValue("y", out var yObj))
        {
            Console.WriteLine("CLICK_COORDS command requires 'x' and 'y' arguments.");
            return;
        }

        if (xObj is int x && yObj is int y)
            Console.WriteLine($"Clicking coordinates: ({x}, {y})");
        else
        {
            Console.WriteLine("Invalid 'x' or 'y' arguments for CLICK_COORDS. Must be integers.");
        }
    }

    private async Task HandleWait(Command command)
    {
        if (!command.Args.TryGetValue("seconds", out var secondsObj))
        {
            Console.WriteLine("WAIT command requires a 'seconds' argument.");
            return;
        }

        if (secondsObj is double seconds)
        {
            Console.WriteLine($"Waiting for {seconds} seconds...");
            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }
        else
        {
            Console.WriteLine("Invalid 'seconds' argument for WAIT. Must be a number.");
        }
    }

    private void HandleScreenshot(Command command)
    {
        if (!command.Args.TryGetValue("filename", out var filenameObj))
        {
            Console.WriteLine("SCREENSHOT command requires a 'filename' argument.");
            return;
        }

        var filename = filenameObj.ToString();
        var inlineBase64 = command.Args.TryGetValue("inline", out var inlineObj) && (bool)inlineObj;

        Console.WriteLine($"Taking screenshot: {filename} (inline: {inlineBase64})");
        var result = _screenshotUtility.TakeScreenshot(filename, inlineBase64);
        Console.WriteLine($"Screenshot result: {result}"); // This result should be sent back to the controller
    }

    private void HandlePing() => Console.WriteLine("PONG");// In a real scenario, this would send a response back via the transport

    private void HandleExit()
    {
        Console.WriteLine("Exiting game...");
        _game.Exit();
    }
}
