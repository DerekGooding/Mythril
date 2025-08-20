namespace Mythril.GameLogic;

public class PartyManager
{
    public List<Character> PartyMembers { get; } = [];

    public PartyManager(ResourceManager resourceManager)
    {
        PartyMembers.AddRange(resourceManager.Characters);

        foreach (var character in PartyMembers)
        {
            character.Job = resourceManager.Jobs.FirstOrDefault(j => j.Name == character.JobName);
        }
    }
}
