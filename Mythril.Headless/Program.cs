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

        // 1. Initialize Content (Manual load for Headless)
        var items = ContentHost.GetContent<Items>();
        var quests = ContentHost.GetContent<Quests>();
        var stats = ContentHost.GetContent<Stats>();
        var abilities = ContentHost.GetContent<CadenceAbilities>();
        var locations = ContentHost.GetContent<Locations>();
        var questDetails = ContentHost.GetContent<QuestDetails>();
        var questUnlocks = ContentHost.GetContent<QuestUnlocks>();
        var questToCadenceUnlocks = ContentHost.GetContent<QuestToCadenceUnlocks>();
        var cadences = ContentHost.GetContent<Cadences>();

        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        string? rootDir = currentDir;
        while (rootDir != null && !File.Exists(Path.Combine(rootDir, "Mythril.sln")))
        {
            rootDir = Path.GetDirectoryName(rootDir);
        }
        
        if (rootDir == null)
        {
            Console.WriteLine("Error: Could not find solution root.");
            Environment.Exit(1);
        }

        string dataDir = Path.Combine(rootDir, "Mythril.Blazor/wwwroot/data");
        
        items.Load(JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(Path.Combine(dataDir, "items.json"))) ?? []);
        quests.Load(JsonConvert.DeserializeObject<List<Quest>>(File.ReadAllText(Path.Combine(dataDir, "quests.json"))) ?? []);
        stats.Load(JsonConvert.DeserializeObject<List<Stat>>(File.ReadAllText(Path.Combine(dataDir, "stats.json"))) ?? []);
        abilities.Load(JsonConvert.DeserializeObject<List<CadenceAbility>>(File.ReadAllText(Path.Combine(dataDir, "cadence_abilities.json"))) ?? []);

        var locDTOs = JsonConvert.DeserializeObject<List<LocationDTO>>(File.ReadAllText(Path.Combine(dataDir, "locations.json"))) ?? [];
        locations.Load(locDTOs.Select(d => new Location(d.Name, d.Quests.Select(qn => quests.All.First(q => q.Name == qn)))).ToList());

        var cadDTOs = JsonConvert.DeserializeObject<List<CadenceDTO>>(File.ReadAllText(Path.Combine(dataDir, "cadences.json"))) ?? [];
        cadences.Load(cadDTOs.Select(d => new Cadence(d.Name, d.Description, d.Abilities.Select(a => new CadenceUnlock(
            abilities.All.First(ab => ab.Name == a.Ability),
            a.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray()
        )).ToArray())).ToList());

        var detailDTOs = JsonConvert.DeserializeObject<List<QuestDetailDTO>>(File.ReadAllText(Path.Combine(dataDir, "quest_details.json"))) ?? [];
        questDetails.Load(detailDTOs.ToDictionary(
            d => quests.All.First(q => q.Name == d.Quest),
            d => new QuestDetail(d.DurationSeconds, 
                d.Requirements.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                d.Rewards.Select(r => new ItemQuantity(items.All.First(i => i.Name == r.Item), r.Quantity)).ToArray(),
                Enum.Parse<QuestType>(d.Type)
            )
        ));

        var unlockDTOs = JsonConvert.DeserializeObject<List<QuestUnlockDTO>>(File.ReadAllText(Path.Combine(dataDir, "quest_unlocks.json"))) ?? [];
        questUnlocks.Load(unlockDTOs.ToDictionary(
            d => quests.All.First(q => q.Name == d.Quest),
            d => d.Requires.Select(rn => quests.All.First(q => q.Name == rn)).ToArray()
        ));

        // 2. Initialize Engine
        var resourceManager = new ResourceManager(items, questUnlocks, questToCadenceUnlocks, questDetails, cadences, locations);
        resourceManager.Initialize();

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
                    var cadence = cadences.All.FirstOrDefault(c => c.Name.Equals(cmd.Target, StringComparison.OrdinalIgnoreCase));
                    if (cadence.Name != null) resourceManager.UnlockCadence(cadence);
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
                    passed = unlocked == (assertion.ExpectedValue == 1);
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
