using System.Collections.Generic;

namespace Mythril.GameLogic
{
    public class PartyManager
    {
        public List<Character> PartyMembers { get; } = new();

        public PartyManager(ResourceManager resourceManager)
        {
            PartyMembers.AddRange(resourceManager.Characters);
        }
    }
}
