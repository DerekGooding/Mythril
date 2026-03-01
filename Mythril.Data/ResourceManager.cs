namespace Mythril.Data;

public class ResourceManager(
    Items items, 
    QuestUnlocks questUnlocks, 
    QuestToCadenceUnlocks questToCadenceUnlocks, 
    QuestDetails questDetails,
    Cadences cadences,
    Locations locations)
{
    private readonly Items _items = items;
    private readonly QuestUnlocks _questUnlocks = questUnlocks;
    private readonly QuestToCadenceUnlocks _questToCadenceUnlocks = questToCadenceUnlocks;
    private readonly QuestDetails _questDetails = questDetails;
    private readonly Cadences _cadences = cadences;
    private readonly Locations _locations = locations;

    public QuestDetail GetQuestDetails(Quest quest) => _questDetails[quest];

    private Dictionary<Cadence, Character?> _assignedCadences = [];
    private Dictionary<Cadence, bool> _lockedCadences = [];

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];

    public InventoryManager Inventory { get; } = new InventoryManager();

    private readonly HashSet<Quest> _completedQuests = [];

    public List<LocationData> UsableLocations = [];

    public List<Cadence> UnlockedCadences = [];
    public List<CadenceAbility> UnlockedAbilities = [];

    public List<QuestProgress> ActiveQuests { get; } = [];

    public void Initialize()
    {
        Console.WriteLine("ResourceManager initializing...");
        Inventory.Clear();
        var gold = _items.All.FirstOrDefault(x => x.Name == "Gold");
        if (gold.Name != null) Inventory.Add(gold, 100);

        Console.WriteLine("Initializing Cadences...");
        _assignedCadences = _cadences.All.ToNamedDictionary(_ => (Character?)null);
        _lockedCadences = _cadences.All.ToNamedDictionary(_ => true);

        Console.WriteLine("Initializing Locations...");
        UsableLocations = [.. _locations.All.Select(x => new LocationData(x, x.Quests.Where(IsNeverLocked)))];
        
        UpdateAvaiableCadences();
        Console.WriteLine("ResourceManager initialized.");
    }

    public bool IsNeverLocked(Quest quest) => _questUnlocks[quest].Length == 0;

    public bool CanAfford(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var requirement in quest.Requirements)
            {
                if (!Inventory.Has(requirement.Item, requirement.Quantity))
                {
                    return false;
                }
            }
        }

        if(item is CadenceUnlock ability)
        {
            foreach(var requirement in ability.Requirements)
            {
                if (!Inventory.Has(requirement.Item, requirement.Quantity))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void UpdateAvaiableCadences()
        => UnlockedCadences = [.. _lockedCadences.Where(x => !x.Value).Select(x => x.Key)];

    private void LockQuest(Quest quest)
    {
        foreach(var location in UsableLocations)
        {
            location.Quests.Remove(quest);
        }
    }

    private void UnlockQuest(Quest quest)
    {
        _completedQuests.Add(quest);
        foreach(var location in UsableLocations)
        {
            foreach(var data in location.LockedQuests)
            {
                if (_questDetails[quest].Type == QuestType.Single && _completedQuests.Contains(data))
                    continue;
                if (IsComplete(_questUnlocks[data]))
                    location.Quests.Add(data);
            }
        }
    }

    private bool IsComplete(Quest[] quests) => quests.All(_completedQuests.Contains);

    public void PayCosts(object item)
    {
        if (item is QuestData quest)
        {
            if (quest.Type == QuestType.Single)
            {
                LockQuest(quest.Quest);
            }

            foreach (var requirement in quest.Requirements)
                Inventory.Remove(requirement.Item, requirement.Quantity);
        }
        if(item is CadenceUnlock unlock)
        {
            foreach (var requirement in unlock.Requirements)
                Inventory.Remove(requirement.Item, requirement.Quantity);
        }
    }

    public void StartQuest(object item, Character character)
    {
        if (CanAfford(item))
        {
            PayCosts(item);
            
            int duration = 3;
            if (item is QuestData quest)
            {
                duration = quest.DurationSeconds;
                // Stat influence: Strength reduces recurring quest duration
                if (quest.Type == QuestType.Recurring)
                {
                    int strength = GetStatValue(character, "Strength");
                    duration = (int)(duration / (1.0 + (strength / 100.0)));
                }
                ActiveQuests.Add(new QuestProgress(quest, quest.Description, duration, character));
            }
            if(item is CadenceUnlock unlock)
            {
                duration = 3;
                // Magic reduces cadence unlock duration
                int magic = GetStatValue(character, "Magic");
                duration = (int)(duration / (1.0 + (magic / 100.0)));
                ActiveQuests.Add(new QuestProgress(unlock, unlock.Ability.Description, duration, character));
            }
        }
    }

    private int GetStatValue(Character character, string statName)
    {
        if (character.Name == "Protagonist") return 10;
        if (character.Name == "Wifu" && statName == "Magic") return 15;
        if (character.Name == "Himbo" && statName == "Strength") return 20;
        return 5;
    }

    public void Tick(double deltaSeconds)
    {
        foreach (var progress in ActiveQuests)
        {
            if (!progress.IsCompleted)
            {
                progress.SecondsElapsed += (int)(deltaSeconds * 10); // Adjusting for 0.1s ticks
            }
        }
    }

    public async Task ReceiveRewards(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var reward in quest.Rewards)
                Inventory.Add(reward.Item, reward.Quantity);

            UnlockQuest(quest.Quest);

            foreach (var cadence in _questToCadenceUnlocks[quest.Quest])
                UnlockCadence(cadence);
        }
        if(item is CadenceUnlock unlock)
        {
            UnlockedAbilities.Add(unlock.Ability);
        }
        await Task.CompletedTask;
    }

    public void UnlockCadence(Cadence cadence)
    {
        _lockedCadences[cadence] = false;
        UpdateAvaiableCadences();
    }

    public void AssignCadence(Cadence cadence, Character character) => _assignedCadences[cadence] = character;
    public void Unassign(Cadence cadence) => _assignedCadences[cadence] = null;
    public IEnumerable<Cadence> CurrentlyAssigned(Character character)
        => _assignedCadences.Where(x => x.Value == character).Select(x => x.Key);
}
