namespace Mythril.Controller.Agents;

public class GeminiCliAdapter : IAgentAdapter
{
    public Task<string[]> GetCommandsAsync(string prompt)
    {
        Console.WriteLine($"[Gemini-CLI] Prompt: {prompt}");
        Console.Write("[Gemini-CLI] Enter commands (JSON array of strings): ");
        var input = Console.ReadLine();
        if (string.IsNullOrEmpty(input))
            return Task.FromResult(Array.Empty<string>());

        try
        {
            var commands = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(input);
            return Task.FromResult(commands ?? []);
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            Console.WriteLine($"[Gemini-CLI] Error parsing commands: {ex.Message}");
            return Task.FromResult(Array.Empty<string>());
        }
    }

    public Task SendResponseAsync(string response)
    {
        Console.WriteLine($"[Gemini-CLI] Response: {response}");
        return Task.CompletedTask;
    }

    public Task SendImageAsync(byte[] imageData)
    {
        Console.WriteLine($"[Gemini-CLI] Received image data (length: {imageData.Length}).");
        // In a real scenario, this would send the image to the Gemini-CLI for display
        return Task.CompletedTask;
    }
}
