using System.Collections.Generic;

namespace Mythril.GameLogic
{
    public class PartyManager
    {
        public List<Character> PartyMembers { get; } = new();

        public PartyManager()
        {
            // Create a default party for now
            PartyMembers.Add(new Character("Hero", "Squire"));
            PartyMembers.Add(new Character("Healer", "Chemist"));
            PartyMembers.Add(new Character("Mage", "Wizard"));
            PartyMembers.Add(new Character("Ranger", "Archer"));
        }
    }
}
