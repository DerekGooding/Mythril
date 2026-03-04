namespace Mythril.Data;

public partial class ResourceManager(
    Items items, 
    QuestUnlocks questUnlocks, 
    QuestToCadenceUnlocks questToCadenceUnlocks, 
    QuestDetails questDetails,
    Cadences cadences,
    Locations locations,
    JunctionManager junctionManager,
    InventoryManager inventory,
    ItemRefinements refinements)
{
    private readonly Items _items = items;
    private readonly QuestUnlocks _questUnlocks = questUnlocks;
    private readonly QuestToCadenceUnlocks _questToCadenceUnlocks = questToCadenceUnlocks;
    private readonly QuestDetails _questDetails = questDetails;
    private readonly Cadences _cadences = cadences;
    private readonly Locations _locations = locations;
    private readonly ItemRefinements _refinements = refinements;
    public JunctionManager JunctionManager { get; } = junctionManager;
    public InventoryManager Inventory { get; } = inventory;

    private readonly object _questLock = new();

    public QuestDetail GetQuestDetails(Quest quest) => _questDetails[quest];

    private Dictionary<Cadence, bool> _lockedCadences = [];

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];

    private readonly HashSet<Quest> _completedQuests = [];

    public List<LocationData> UsableLocations = [];
    public readonly HashSet<string> UnlockedLocationNames = [];

    public List<Cadence> UnlockedCadences = [];
    public List<string> UnlockedCadenceNames = [];
    public HashSet<string> UnlockedAbilities = [];

    public List<QuestProgress> ActiveQuests { get; } = [];

    private readonly Dictionary<string, bool> _autoQuestEnabled = [];
    public IReadOnlyDictionary<string, bool> AutoQuestEnabled => _autoQuestEnabled;

    public bool IsTestMode { get; set; } = false;

    public bool HasUnseenCadence { get; set; } = false;
    public bool HasUnseenWorkshop { get; set; } = false;
    public string ActiveTab { get; set; } = "hand";

    public void Initialize()
    {
        Console.WriteLine("ResourceManager initializing...");
        Inventory.Clear();
        _completedQuests.Clear();
        UnlockedAbilities.Clear();
        _autoQuestEnabled.Clear();
        UnlockedLocationNames.Clear();
        HasUnseenCadence = false;
        HasUnseenWorkshop = false;
        lock(_questLock)
        {
            ActiveQuests.Clear();
        }
        
        var gold = _items.All.FirstOrDefault(x => x.Name == "Gold");
        if (gold.Name != null) Inventory.Add(gold, 100);

        Console.WriteLine("Initializing Cadences...");
        _lockedCadences = _cadences.All.ToNamedDictionary(_ => true);

        Console.WriteLine("Initializing Locations...");
        UpdateUsableLocations();
        
        UpdateAvaiableCadences();
        JunctionManager.Initialize();
        Console.WriteLine("ResourceManager initialized.");
    }

    public void UpdateUsableLocations()
    {
        UsableLocations = [.. _locations.All
            .Where(l => string.IsNullOrEmpty(l.RequiredQuest) || UnlockedLocationNames.Contains(l.Name) || _completedQuests.Any(q => q.Name == l.RequiredQuest))
            .Select(x => new LocationData(x, x.Quests.Where(IsNeverLocked)))];
        
        foreach(var location in UsableLocations)
        {
            if (!string.IsNullOrEmpty(location.Name))
                UnlockedLocationNames.Add(location.Name);
        }
    }

    public bool CanAutoQuest(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        return assigned.Any(c => c.Abilities.Any(a => UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}") && a.Ability.Name == "AutoQuest I"));
    }

    public bool IsAutoQuestEnabled(Character character) => _autoQuestEnabled.TryGetValue(character.Name, out var enabled) && enabled;

    public void SetAutoQuestEnabled(Character character, bool enabled) => _autoQuestEnabled[character.Name] = enabled;

    public void UpdateAvaiableCadences()
    {
        UnlockedCadences = [.. _lockedCadences.Where(x => !x.Value).Select(x => x.Key)];
        UnlockedCadenceNames = [.. UnlockedCadences.Select(c => c.Name)];
        Console.WriteLine($"Unlocked Cadences Updated: {string.Join(", ", UnlockedCadenceNames)}");
    }

    public void Tick(double deltaSeconds)
    {
        lock(_questLock)
        {
            foreach (var progress in ActiveQuests)
            {
                if (!progress.IsCompleted)
                {
                    progress.SecondsElapsed += deltaSeconds;
                }
            }
        }
    }

    public void UpdateMagicCapacity()
    {
        int capacity = 30;
        if (UnlockedAbilities.Any(a => a.EndsWith(":Magic Pocket I"))) capacity = 60;
        if (UnlockedAbilities.Any(a => a.EndsWith(":Magic Pocket II"))) capacity = 100;
        Inventory.MagicCapacity = capacity;
    }

    public void UnlockCadence(Cadence cadence)
    {
        _lockedCadences[cadence] = false;
        if (ActiveTab != "cadence")
        {
            HasUnseenCadence = true;
        }
        UpdateAvaiableCadences();
    }

    public void RemoveActiveQuest(QuestProgress progress)
    {
        lock(_questLock)
        {
            ActiveQuests.Remove(progress);
        }
    }

    public void CancelQuest(QuestProgress progress)
    {
        lock(_questLock)
        {
            if (ActiveQuests.Contains(progress))
            {
                RefundCosts(progress.Item);
                ActiveQuests.Remove(progress);
            }
        }
    }

    private void RefundCosts(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var requirement in quest.Requirements)
                Inventory.Add(requirement.Item, requirement.Quantity);
        }
        if (item is CadenceUnlock unlock)
        {
            foreach (var requirement in unlock.Requirements)
                Inventory.Add(requirement.Item, requirement.Quantity);
        }
        if (item is RefinementData refinement)
        {
            Inventory.Add(refinement.InputItem, refinement.Recipe.InputQuantity);
        }
    }

    public bool IsInProgress(object item)
    {
        lock(_questLock)
        {
            if (item is QuestData quest)
            {
                return ActiveQuests.Any(p => p.Item is QuestData activeQuest && activeQuest.Quest.Name == quest.Quest.Name);
            }
            if (item is CadenceUnlock unlock)
            {
                return ActiveQuests.Any(p => p.Item is CadenceUnlock activeUnlock && activeUnlock.CadenceName == unlock.CadenceName && activeUnlock.Ability.Name == unlock.Ability.Name);
            }
            if (item is RefinementData refinement)
            {
                return ActiveQuests.Any(p => p.Item is RefinementData activeRefinement && 
                    activeRefinement.Ability.Name == refinement.Ability.Name && 
                    activeRefinement.InputItem.Name == refinement.InputItem.Name);
            }
            return false;
        }
    }

    public IEnumerable<Quest> GetCompletedQuests() => _completedQuests;
    public void ClearCompletedQuests() => _completedQuests.Clear();
}
