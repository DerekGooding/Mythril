using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Mythril.Data;

namespace Mythril.Headless;

public class CommandFile
{
    public List<Command> Commands { get; set; } = [];
}

public class Command
{
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string Character { get; set; } = string.Empty;
}

public class GameState
{
    public List<KeyValuePair<string, int>> Inventory { get; set; } = [];
    public List<string> UnlockedCadences { get; set; } = [];
    public List<string> CompletedQuests { get; set; } = [];
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Mythril.Headless <command_file.json> [output_state.json]");
            return;
        }

        string commandFilePath = args[0];
        string outputFilePath = args.Length > 1 ? args[1] : "state.json";

        if (!File.Exists(commandFilePath))
        {
            Console.WriteLine($"Error: File not found {commandFilePath}");
            return;
        }

        var resourceManager = new ResourceManager();
        var items = ContentHost.GetContent<Items>();
        var quests = ContentHost.GetContent<Quests>();
        var cadenceAbilities = ContentHost.GetContent<CadenceAbilities>();
        var questDetails = ContentHost.GetContent<QuestDetails>();

        var json = File.ReadAllText(commandFilePath);
        var commandFile = JsonConvert.DeserializeObject<CommandFile>(json);

        if (commandFile == null)
        {
            Console.WriteLine("Error: Failed to parse command file.");
            return;
        }

        foreach (var cmd in commandFile.Commands)
        {
            Console.WriteLine($"Executing: {cmd.Action} {cmd.Target}");
            
            switch (cmd.Action.ToLower())
            {
                case "add_item":
                    var item = items.All.FirstOrDefault(i => i.Name.Equals(cmd.Target, StringComparison.OrdinalIgnoreCase));
                    if (item.Name != null) resourceManager.Inventory.Add(item, cmd.Quantity);
                    break;
                
                case "complete_quest":
                    var quest = quests.All.FirstOrDefault(q => q.Name.Equals(cmd.Target, StringComparison.OrdinalIgnoreCase));
                    if (quest.Name != null)
                    {
                        var detail = questDetails[quest];
                        var questData = new QuestData(quest, detail);
                        resourceManager.ReceiveRewards(questData).Wait();
                    }
                    break;
                
                case "unlock_cadence":
                    // Simple unlock for testing
                    var cadence = ContentHost.GetContent<Cadences>().All.FirstOrDefault(c => c.Name.Equals(cmd.Target, StringComparison.OrdinalIgnoreCase));
                    if (cadence.Name != null) resourceManager.UnlockCadence(cadence);
                    break;
            }
        }

        var finalState = new GameState
        {
            Inventory = resourceManager.Inventory.GetItems().Select(i => new KeyValuePair<string, int>(i.Item.Name, i.Quantity)).ToList(),
            UnlockedCadences = resourceManager.UnlockedCadences.Select(c => c.Name).ToList(),
            // Completed quests are private in RM, but we can see the results (rewards/unlocks)
        };

        File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(finalState, Formatting.Indented));
        Console.WriteLine($"Final state saved to {outputFilePath}");
    }
}
