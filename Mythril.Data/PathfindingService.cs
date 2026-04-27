namespace Mythril.Data;

public class PathfindingService(
    Locations locations,
    Quests quests,
    QuestUnlocks questUnlocks,
    QuestDetails questDetails,
    Cadences cadences,
    QuestToCadenceUnlocks questToCadenceUnlocks)
{
    public HashSet<string> GetPrerequisitePath(string targetId, IEnumerable<string> completedQuests, IEnumerable<string> unlockedAbilities)
    {
        var completedSet = completedQuests.ToHashSet();
        var unlockedSet = unlockedAbilities.ToHashSet();
        var path = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(targetId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (path.Contains(currentId)) continue;
            path.Add(currentId);

            // If it's a quest ID
            var quest = quests.All.FirstOrDefault(q => q.Name == currentId);
            if (quest.Name != null && !completedSet.Contains(quest.Name))
            {
                // Prerequisite Quests
                foreach (var pre in questUnlocks[quest])
                {
                    if (!completedSet.Contains(pre.Name)) queue.Enqueue(pre.Name);
                }

                // Location Requirement
                var loc = locations.All.FirstOrDefault(l => l.Quests?.Any(q => q.Name == quest.Name) == true);
                if (loc.Name != null && !string.IsNullOrEmpty(loc.RequiredQuest) && !completedSet.Contains(loc.RequiredQuest))
                {
                    queue.Enqueue(loc.RequiredQuest);
                }

                // Item Requirements (simplified: just showing the first source for each item)
                var detail = questDetails[quest];
                if (detail.Requirements != null)
                {
                    foreach (var req in detail.Requirements)
                    {
                        // Find a source for this item
                        var source = FindSourceForItem(req.Item.Name, completedSet);
                        if (source != null && !completedSet.Contains(source) && !unlockedSet.Contains(source))
                        {
                            queue.Enqueue(source);
                        }
                    }
                }
            }

            // If it's an ability (CadenceName:AbilityName)
            if (currentId.Contains(':'))
            {
                var parts = currentId.Split(':');
                var cadenceName = parts[0];
                var abilityName = parts[1];

                if (!unlockedSet.Contains(currentId))
                {
                    var cadence = cadences.All.FirstOrDefault(c => c.Name == cadenceName);
                    if (cadence.Name != null)
                    {
                        // Cadence must be unlocked (usually via quest)
                        var unlockingQuest = questToCadenceUnlocks.ByKey.FirstOrDefault(kv => kv.Value.Any(c => c.Name == cadenceName)).Key;
                        if (unlockingQuest.Name != null && !completedSet.Contains(unlockingQuest.Name))
                        {
                            queue.Enqueue(unlockingQuest.Name);
                        }

                        // Ability requirements
                        var unlock = cadence.Abilities.FirstOrDefault(a => a.Ability.Name == abilityName);
                        if (unlock.Ability.Name != null && unlock.Requirements != null)
                        {
                            foreach (var req in unlock.Requirements)
                            {
                                var source = FindSourceForItem(req.Item.Name, completedSet);
                                if (source != null && !completedSet.Contains(source) && !unlockedSet.Contains(source))
                                {
                                    queue.Enqueue(source);
                                }
                            }
                        }
                    }
                }
            }
        }

        return path;
    }

    private string? FindSourceForItem(string itemName, IEnumerable<string> completedQuests)
    {
        ArgumentNullException.ThrowIfNull(completedQuests);
        // Try to find a quest that rewards it
        var sourceQuest = questDetails.ByKey.FirstOrDefault(kv => kv.Value.Rewards?.Any(r => r.Item.Name == itemName) == true).Key;
        return sourceQuest.Name != null ? sourceQuest.Name : null;
    }
}