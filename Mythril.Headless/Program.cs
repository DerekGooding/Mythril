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
    public List<Assertion> Assertions { get; set; } = [];
}

public class Command
{
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string Character { get; set; } = string.Empty;
}

public class Assertion
{
    public string Type { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int ExpectedValue { get; set; }
}

public class GameState
{
    public List<KeyValuePair<string, int>> Inventory { get; set; } = [];
    public List<string> UnlockedCadences { get; set; } = [];
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Mythril.Headless <command_file.json> [output_state.json]");
            Environment.Exit(1);
        }

        string commandFilePath = args[0];
        string outputFilePath = args.Length > 1 ? args[1] : "state.json";

        if (!File.Exists(commandFilePath))
        {
            Console.WriteLine($"Error: File not found {commandFilePath}");
            Environment.Exit(1);
        }

        var resourceManager = new ResourceManager();
        var items = ContentHost.GetContent<Items>();
        var quests = ContentHost.GetContent<Quests>();
        var questDetails = ContentHost.GetContent<QuestDetails>();

        var json = File.ReadAllText(commandFilePath);
        var commandFile = JsonConvert.DeserializeObject<CommandFile>(json);

        if (commandFile == null)
        {
            Console.WriteLine("Error: Failed to parse command file.");
            Environment.Exit(1);
        }

        foreach (var cmd in commandFile.Commands)
        {
            Console.WriteLine($"Executing: {cmd.Action} {cmd.Target}");
            
            switch (cmd.Action.ToLower())
            {
                case "add_item":
                    var item = items.All.FirstOrDefault(i => i.Name.Equals(cmd.Target, StringComparison.OrdinalIgnoreCase));
                    if (item.Name != null) resourceManager.Inventory.Add(item, cmd.Quantity);
                    else Console.WriteLine($"Warning: Item '{cmd.Target}' not found.");
                    break;
                
                case "complete_quest":
                    var quest = quests.All.FirstOrDefault(q => q.Name.Equals(cmd.Target, StringComparison.OrdinalIgnoreCase));
                    if (quest.Name != null)
                    {
                        var detail = questDetails[quest];
                        var questData = new QuestData(quest, detail);
                        resourceManager.ReceiveRewards(questData).Wait();
                    }
                    else Console.WriteLine($"Warning: Quest '{cmd.Target}' not found.");
                    break;
                
                case "unlock_cadence":
                    var cadence = ContentHost.GetContent<Cadences>().All.FirstOrDefault(c => c.Name.Equals(cmd.Target, StringComparison.OrdinalIgnoreCase));
                    if (cadence.Name != null) resourceManager.UnlockCadence(cadence);
                    else Console.WriteLine($"Warning: Cadence '{cmd.Target}' not found.");
                    break;
            }
        }

        var finalState = new GameState
        {
            Inventory = resourceManager.Inventory.GetItems().Select(i => new KeyValuePair<string, int>(i.Item.Name, i.Quantity)).ToList(),
            UnlockedCadences = resourceManager.UnlockedCadences.Select(c => c.Name).ToList(),
        };

        File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(finalState, Formatting.Indented));
        Console.WriteLine($"Final state saved to {outputFilePath}");

        // Assertions
        bool allPassed = true;
        foreach (var assertion in commandFile.Assertions)
        {
            bool passed = false;
            switch (assertion.Type.ToLower())
            {
                case "inventorycount":
                    var item = items.All.FirstOrDefault(i => i.Name.Equals(assertion.Target, StringComparison.OrdinalIgnoreCase));
                    int count = resourceManager.Inventory.GetQuantity(item);
                    passed = count == assertion.ExpectedValue;
                    Console.WriteLine($"Assertion: InventoryCount {assertion.Target} expected {assertion.ExpectedValue}, got {count} - {(passed ? "PASS" : "FAIL")}");
                    break;
                case "cadenceunlocked":
                    bool unlocked = resourceManager.UnlockedCadences.Any(c => c.Name.Equals(assertion.Target, StringComparison.OrdinalIgnoreCase));
                    passed = unlocked == (assertion.ExpectedValue == 1); // 1 for true, 0 for false
                    Console.WriteLine($"Assertion: CadenceUnlocked {assertion.Target} expected {assertion.ExpectedValue == 1}, got {unlocked} - {(passed ? "PASS" : "FAIL")}");
                    break;
                default:
                    Console.WriteLine($"Warning: Unknown assertion type '{assertion.Type}'");
                    break;
            }
            if (!passed) allPassed = false;
        }

        if (!allPassed)
        {
            Console.WriteLine("One or more assertions failed.");
            Environment.Exit(1);
        }
    }
}
