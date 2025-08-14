using System;
using System.Threading.Tasks;

namespace Mythril.Controller.Agents
{
    public class GeminiCliAdapter : IAgentAdapter
    {
        public Task<string[]> GetCommandsAsync(string prompt)
        {
            Console.WriteLine($"[Gemini-CLI] Prompt: {prompt}");
            Console.Write("[Gemini-CLI] Enter commands (JSON array of strings): ");
            var input = Console.ReadLine();
            try
            {
                var commands = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(input);
                return Task.FromResult(commands ?? new string[0]);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Console.WriteLine($"[Gemini-CLI] Error parsing commands: {ex.Message}");
                return Task.FromResult(new string[0]);
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
}
