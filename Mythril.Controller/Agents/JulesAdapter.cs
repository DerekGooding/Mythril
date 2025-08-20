namespace Mythril.Controller.Agents;

public class JulesAdapter : IAgentAdapter
{
    private static readonly string[] _result = ["{\"action\": \"PING\"}"];

    public Task<string[]> GetCommandsAsync(string prompt)
    {
        Console.WriteLine($"[Jules] Prompt: {prompt}");
        // In a real scenario, this would call Jules' task API to get commands.
        // For now, returning a hardcoded command for demonstration.
        return Task.FromResult(_result);
    }

    public Task SendResponseAsync(string response)
    {
        Console.WriteLine($"[Jules] Response: {response}");
        // In a real scenario, this would send the response back to Jules' task API.
        return Task.CompletedTask;
    }

    public Task SendImageAsync(byte[] imageData)
    {
        Console.WriteLine($"[Jules] Received image data (length: {imageData.Length}).");
        // In a real scenario, this would send the image to Jules in the format it expects.
        return Task.CompletedTask;
    }
}
