using System.Linq;

namespace Mythril.Data
{
    public partial class Items
    {
        public Item Gold => All.First(x => x.Name == "Gold");
        public Item Potion => All.First(x => x.Name == "Potion");
        public Item BasicGem => All.First(x => x.Name == "Basic Gem");
    }
}
