using System.Linq;

namespace Mythril.Data
{
    public partial class Quests
    {
        public Quest Prologue => All.First(x => x.Name == "Prologue");
        public Quest TutorialSection => All.First(x => x.Name == "Tutorial Section");
        public Quest VisitStartingTown => All.First(x => x.Name == "Visit Starting Town");
        public Quest BuyPotion => All.First(x => x.Name == "Buy Potion");
        public Quest UnlockStrengthJunction => All.First(x => x.Name == "Unlock Strength Junction");
        public Quest UnlockFireRefineAbility => All.First(x => x.Name == "Unlock Fire Refine Ability");
        public Quest FarmGoblins => All.First(x => x.Name == "Farm Goblins");
    }
}
