using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mythril.Data;

public partial class ResourceManager
{
    private readonly GameStore _gameStore;
    private readonly Items _items;
    private readonly Quests _quests;
    private readonly QuestUnlocks _questUnlocks;
    private readonly QuestToCadenceUnlocks _questToCadenceUnlocks;
    private readonly QuestDetails _questDetails;
    private readonly Cadences _cadences;
    private readonly Locations _locations;
    private readonly ItemRefinements _refinements;
    private readonly PathfindingService _pathfinding;
    public JunctionManager JunctionManager { get; }
    public InventoryManager Inventory { get; }

    public ResourceManager(
        GameStore gameStore,
        Items items, 
        Quests quests,
        QuestUnlocks questUnlocks, 
        QuestToCadenceUnlocks questToCadenceUnlocks, 
        QuestDetails questDetails,
        Cadences cadences,
        Locations locations,
        JunctionManager junctionManager,
        InventoryManager inventory,
        ItemRefinements refinements,
        PathfindingService pathfinding)
    {
        _gameStore = gameStore;
        _items = items;
        _quests = quests;
        _questUnlocks = questUnlocks;
        _questToCadenceUnlocks = questToCadenceUnlocks;
        _questDetails = questDetails;
        _cadences = cadences;
        _locations = locations;
        _refinements = refinements;
        _pathfinding = pathfinding;
        JunctionManager = junctionManager;
        Inventory = inventory;

        JunctionManager.OnCadenceUnassigned += CancelExcessQuests;
        _gameStore.OnItemOverflow += (name, qty) => OnItemOverflow?.Invoke(name, qty);
    }

    private readonly object _questLock = new();

    public QuestDetail GetQuestDetails(Quest quest) => _questDetails[quest];

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];

    public List<LocationData> UsableLocations => _locations.All
        .Where(l => string.IsNullOrEmpty(l.RequiredQuest) || _gameStore.State.CompletedQuests.Contains(l.RequiredQuest))
        .Select(l => new LocationData(
            l,
            l.Quests.Where(q => {
                var detail = _questDetails[q];
                bool meetsPrereqs = _questUnlocks[q].All(req => _gameStore.State.CompletedQuests.Contains(req.Name));
                bool alreadyCompleted = _gameStore.State.CompletedQuests.Contains(q.Name);
                bool isSingle = detail.Type == QuestType.Single || detail.Type == QuestType.Unlock;
                return meetsPrereqs && !(isSingle && alreadyCompleted);
            })
        )).ToList();

    public HashSet<string> UnlockedLocationNames => _gameStore.State.UnlockedLocationNames.ToHashSet();

    public List<Cadence> UnlockedCadences => _gameStore.State.UnlockedCadenceNames.Select(name => _cadences.All.First(c => c.Name == name)).ToList();
    public List<string> UnlockedCadenceNames => _gameStore.State.UnlockedCadenceNames.ToList();
    public HashSet<string> UnlockedAbilities => _gameStore.State.UnlockedAbilities.ToHashSet();

    public HashSet<string> HighlightedPath => _gameStore.State.HighlightedPath.ToHashSet();

    public void HighlightPath(string targetId)
    {
        var path = _pathfinding.GetPrerequisitePath(targetId, [.. _gameStore.State.CompletedQuests], _gameStore.State.UnlockedAbilities);
        _gameStore.Dispatch(new SetHighlightedPathAction(path.ToImmutableHashSet()));
    }

    public void ClearHighlight()
    {
        _gameStore.Dispatch(new ClearHighlightedPathAction());
    }

    public List<QuestProgress> ActiveQuests => _gameStore.State.ActiveQuests.ToList();

    public IReadOnlyDictionary<string, bool> AutoQuestEnabled => _gameStore.State.AutoQuestEnabled;

    public HashSet<string> StarredRecipes => _gameStore.State.StarredRecipes.ToHashSet();

    public void ToggleRecipeStar(string recipeKey)
    {
        _gameStore.Dispatch(new ToggleRecipeStarAction(recipeKey));
    }

    public bool IsTestMode { get; set; } = false;

    public bool HasUnseenCadence { get; set; } = false;
    public bool HasUnseenWorkshop { get; set; } = false;
    public string ActiveTab { get; set; } = "hand";

    public event Action<string, int>? OnItemOverflow;

    public void Initialize()
    {
        Console.WriteLine("ResourceManager initializing...");
        _gameStore.Dispatch(new SetStateAction(GameState.Initial));
        
        Console.WriteLine("Initializing Locations...");
        UpdateUsableLocations();
        
        Console.WriteLine("ResourceManager initialized.");
    }

    public ItemRefinements Refinements => _refinements;

    public Location GetLocation(string name) => _locations.All.FirstOrDefault(l => l.Name == name) is var l && l.Name != null ? l : new Location(name, []);

    public void Tick(double deltaSeconds)
    {
        _gameStore.Dispatch(new TickAction(deltaSeconds));
        CheckAutoQuestTick();
        CheckHiddenCadences();
    }

    public IEnumerable<Quest> GetCompletedQuests() => _gameStore.State.CompletedQuests.Select(name => _quests.All.FirstOrDefault(q => q.Name == name) is var q && q.Name != null ? q : new Quest(name, ""));
    public void ClearCompletedQuests() 
    {
        foreach(var q in _gameStore.State.CompletedQuests)
        {
            _gameStore.Dispatch(new LockQuestAction(new Quest(q, "")));
        }
    }
}
