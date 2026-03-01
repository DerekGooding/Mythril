namespace Mythril.Data;

public class ResourceManager(
    Items items, 
    QuestUnlocks questUnlocks, 
    QuestToCadenceUnlocks questToCadenceUnlocks, 
    QuestDetails questDetails,
    Cadences cadences,
    Locations locations,
    JunctionManager junctionManager,
    InventoryManager inventory)
{
    private readonly Items _items = items;
    private readonly QuestUnlocks _questUnlocks = questUnlocks;
    private readonly QuestToCadenceUnlocks _questToCadenceUnlocks = questToCadenceUnlocks;
    private readonly QuestDetails _questDetails = questDetails;
    private readonly Cadences _cadences = cadences;
    private readonly Locations _locations = locations;
    public JunctionManager JunctionManager { get; } = junctionManager;
    public InventoryManager Inventory { get; } = inventory;

    public QuestDetail GetQuestDetails(Quest quest) => _questDetails[quest];

    private Dictionary<Cadence, bool> _lockedCadences = [];

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];

    private readonly HashSet<Quest> _completedQuests = [];

    public List<LocationData> UsableLocations = [];

    public List<Cadence> UnlockedCadences = [];
    public List<CadenceAbility> UnlockedAbilities = [];

    public List<QuestProgress> ActiveQuests { get; } = [];

    public bool IsTestMode { get; set; } = false;

    public void Initialize()
    {
        Console.WriteLine("ResourceManager initializing...");
        Inventory.Clear();
        var gold = _items.All.FirstOrDefault(x => x.Name == "Gold");
        if (gold.Name != null) Inventory.Add(gold, 100);

        Console.WriteLine("Initializing Cadences...");
        _lockedCadences = _cadences.All.ToNamedDictionary(_ => true);

        Console.WriteLine("Initializing Locations...");
        UsableLocations = [.. _locations.All.Select(x => new LocationData(x, x.Quests.Where(IsNeverLocked)))];
        
        UpdateAvaiableCadences();
        JunctionManager.Initialize();
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
                if (_questDetails[data].Type == QuestType.Single && _completedQuests.Contains(data))
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
            if (quest.Type == QuestType.Single) LockQuest(quest.Quest);

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
            double duration = 10; // Default
            if (item is QuestData quest)
            {
                duration = IsTestMode ? 3 : quest.DurationSeconds;
                if (!IsTestMode)
                {
                    // Strength reduces recurring quest duration
                    if (quest.Type == QuestType.Recurring)
                    {
                        int strength = JunctionManager.GetStatValue(character, "Strength");
                        duration /= (1.0 + (strength / 100.0));
                    }
                    // Vitality reduces single quest duration
                    else if (quest.Type == QuestType.Single)
                    {
                        int vitality = JunctionManager.GetStatValue(character, "Vitality");
                        duration /= (1.0 + (vitality / 100.0));
                    }
                }
                ActiveQuests.Add(new QuestProgress(quest, quest.Description, (int)Math.Max(1, duration), character));
            }
            if(item is CadenceUnlock unlock)
            {
                duration = IsTestMode ? 3 : 30; // Increased base duration for Cadence unlocks
                if (!IsTestMode)
                {
                    // Magic reduces cadence unlock duration
                    int magic = JunctionManager.GetStatValue(character, "Magic");
                    duration /= (1.0 + (magic / 100.0));
                }
                ActiveQuests.Add(new QuestProgress(unlock, unlock.Ability.Description, (int)Math.Max(1, duration), character));
            }
        }
    }

    public void Tick(double deltaSeconds)
    {
        foreach (var progress in ActiveQuests)
        {
            if (!progress.IsCompleted)
            {
                progress.SecondsElapsed += deltaSeconds;
            }
        }
    }

    public async Task ReceiveRewards(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var reward in quest.Rewards) Inventory.Add(reward.Item, reward.Quantity);
            UnlockQuest(quest.Quest);
            foreach (var cadence in _questToCadenceUnlocks[quest.Quest]) UnlockCadence(cadence);
        }
        if(item is CadenceUnlock unlock)
        {
            UnlockedAbilities.Add(unlock.Ability);
            UpdateMagicCapacity();
        }
        await Task.CompletedTask;
    }

    public void UpdateMagicCapacity()
    {
        int capacity = 30;
        if (UnlockedAbilities.Any(a => a.Name == "Magic Pocket I")) capacity = 60;
        Inventory.MagicCapacity = capacity;
    }

    public void UnlockCadence(Cadence cadence)
    {
        _lockedCadences[cadence] = false;
        UpdateAvaiableCadences();
    }

    public IEnumerable<Quest> GetCompletedQuests() => _completedQuests;
    public void ClearCompletedQuests() => _completedQuests.Clear();
    public void RestoreCompletedQuest(Quest quest) => UnlockQuest(quest);
}
