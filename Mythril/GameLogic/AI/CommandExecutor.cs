using Myra.Graphics2D.UI;
using Mythril.API;

namespace Mythril.GameLogic.AI;

public class CommandExecutor(Game1 game, Desktop desktop) : ICommandExecutor
{
    private readonly Game1 _game = game;
    private readonly Desktop _desktop = desktop;

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
                await HandleScreenshot(command);
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

    private Button? FindButtonInTree(Widget? widget, string target)
    {
        if (widget == null) return null;

        if (widget.Id == target && widget is Button buttonById)
        {
            return buttonById;
        }

        if (widget is Button button && button.Content is Label label && label.Text == target)
        {
            return button;
        }

        if (widget is Container container)
        {
            foreach (var child in container.Widgets)
            {
                var found = FindButtonInTree(child, target);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private Button? FindButton(string target)
    {
        if (_desktop.HasModalWidget)
        {
            var modelWidget = _desktop.Widgets.First(x => x.IsModal);
            var button = FindButtonInTree(modelWidget, target);
            if (button != null) return button;
        }

        return FindButtonInTree(_desktop.Root, target);
    }

    private void HandleClickButton(Command command)
    {
        if (command.Target == null)
        {
            Console.WriteLine("CLICK_BUTTON command requires a 'target' (button ID).");
            return;
        }

        var button = FindButton(command.Target);

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

    private async Task HandleScreenshot(Command command)
    {
        var tcs = new TaskCompletionSource<string>();
        _game.RequestScreenshot(tcs.SetResult);
        var screenshotResult = await tcs.Task;
        Console.WriteLine(screenshotResult);
    }

    private void HandlePing() => Console.WriteLine("PONG");

    private void HandleExit()
    {
        Console.WriteLine("Exiting game...");
        _game.Exit();
    }
}
