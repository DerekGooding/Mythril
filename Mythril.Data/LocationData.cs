namespace Mythril.Data;

public class LocationData(Location location, IEnumerable<Quest> startingUnlockedQuests)
{
    private readonly Location _location = location;
    public string Name => _location.Name;

    public List<Quest> Quests { get; set; } = [..startingUnlockedQuests];


    public IEnumerable<Quest> LockedQuests => _location.Quests.Where(q => !Quests.Contains(q));
}
