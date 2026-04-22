using System.Collections.Generic;
using System.Linq;

namespace Mythril.Data;

public partial class ResourceManager
{
    public List<string> GetForwardUnlocks(Quest quest)
    {
        var unlocks = new List<string>();

        // 1. Check if any other quest requires this quest
        if (_questUnlocks.ByKey.Any(kvp => kvp.Value.Any(req => req.Name == quest.Name)))
        {
            unlocks.Add("unlock new quest");
        }

        // 2. Check if this quest unlocks any cadences
        if (_questToCadenceUnlocks.ByKey.TryGetValue(quest, out var cadences) && cadences.Length > 0)
        {
            unlocks.Add("unlock new cadence");
        }

        // 3. Check if this quest unlocks any locations
        if (_locations.All.Any(l => l.RequiredQuest == quest.Name))
        {
            unlocks.Add("unlock new location");
        }

        return unlocks.Distinct().ToList();
    }
}
