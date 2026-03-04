namespace Mythril.Data;

public partial class ResourceManager
{
    private readonly Items _items;
    private readonly QuestUnlocks _questUnlocks;
    private readonly QuestToCadenceUnlocks _questToCadenceUnlocks;
    private readonly QuestDetails _questDetails;
    private readonly Cadences _cadences;
    private readonly Locations _locations;
    private readonly ItemRefinements _refinements;
    public JunctionManager JunctionManager { get; }
    public InventoryManager Inventory { get; }

    public ResourceManager(
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
        _items = items;
        _questUnlocks = questUnlocks;
        _questToCadenceUnlocks = questToCadenceUnlocks;
        _questDetails = questDetails;
        _cadences = cadences;
        _locations = locations;
        _refinements = refinements;
        JunctionManager = junctionManager;
        Inventory = inventory;

        JunctionManager.OnCadenceUnassigned += CancelExcessQuests;
    }

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

    public event Action<string, int>? OnItemOverflow;

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
        
        Console.WriteLine("Initializing Cadences...");
        _lockedCadences = _cadences.All.ToNamedDictionary(_ => true);

        Console.WriteLine("Initializing Locations...");
        UpdateUsableLocations();
        
        UpdateAvaiableCadences();
        JunctionManager.Initialize();
        Console.WriteLine("ResourceManager initialized.");
    }

    public ItemRefinements Refinements => _refinements;

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

    public IEnumerable<Quest> GetCompletedQuests() => _completedQuests;
    public void ClearCompletedQuests() => _completedQuests.Clear();
}
