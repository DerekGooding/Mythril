namespace Mythril.Data;

public class PartyManager
{
    public List<Character> PartyMembers { get; } = [];

    public PartyManager(ResourceManager resourceManager) => PartyMembers.AddRange(resourceManager.Characters);

    public void AddPartyMember(Character character) => PartyMembers.Add(character);
}
