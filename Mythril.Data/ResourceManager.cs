namespace Mythril.Data;

public partial class ResourceManager(
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

    private readonly object _questLock = new();

    public QuestDetail GetQuestDetails(Quest quest) => _questDetails[quest];

    private Dictionary<Cadence, bool> _lockedCadences = [];

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];

    private readonly HashSet<Quest> _completedQuests = [];

    public List<LocationData> UsableLocations = [];

    public List<Cadence> UnlockedCadences = [];
    public List<string> UnlockedCadenceNames = [];
    public List<CadenceAbility> UnlockedAbilities = [];

    public List<QuestProgress> ActiveQuests { get; } = [];

    public bool IsTestMode { get; set; } = false;

    public void Initialize()
    {
        Console.WriteLine("ResourceManager initializing...");
        Inventory.Clear();
        _completedQuests.Clear();
        UnlockedAbilities.Clear();
        lock(_questLock)
        {
            ActiveQuests.Clear();
        }
        
        var gold = _items.All.FirstOrDefault(x => x.Name == "Gold");
        if (gold.Name != null) Inventory.Add(gold, 100);

        Console.WriteLine("Initializing Cadences...");
        _lockedCadences = _cadences.All.ToNamedDictionary(_ => true);

        // Pre-unlock "Test" cadence
        var testCadence = _cadences.All.FirstOrDefault(c => c.Name == "Test");
        if (testCadence.Name != null) _lockedCadences[testCadence] = false;

        Console.WriteLine("Initializing Locations...");
        UsableLocations = [.. _locations.All.Select(x => new LocationData(x, x.Quests.Where(IsNeverLocked)))];
        
        UpdateAvaiableCadences();
        JunctionManager.Initialize();
        Console.WriteLine("ResourceManager initialized.");
    }

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
        if (UnlockedAbilities.Any(a => a.Name == "Magic Pocket I")) capacity = 60;
        Inventory.MagicCapacity = capacity;
    }

    public void UnlockCadence(Cadence cadence)
    {
        _lockedCadences[cadence] = false;
        UpdateAvaiableCadences();
    }

    public void RemoveActiveQuest(QuestProgress progress)
    {
        lock(_questLock)
        {
            ActiveQuests.Remove(progress);
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
                return ActiveQuests.Any(p => p.Item is CadenceUnlock activeUnlock && activeUnlock.Ability.Name == unlock.Ability.Name);
            }
            return false;
        }
    }

    public IEnumerable<Quest> GetCompletedQuests() => _completedQuests;
    public void ClearCompletedQuests() => _completedQuests.Clear();
}
