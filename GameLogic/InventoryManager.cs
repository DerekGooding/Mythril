using Mythril.GameLogic.Items;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.GameLogic
{
    public class InventoryManager
    {
        private readonly List<Item> _items;
        private readonly ResourceManager _resourceManager;

        public IReadOnlyList<Item> Items => _items.AsReadOnly();

        public InventoryManager(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            _items = new List<Item>();
        }

        public void AddItem(string itemName, int quantity = 1)
        {
            var itemToAdd = _resourceManager.Items.FirstOrDefault(i => i.Name == itemName);
            if (itemToAdd != null)
            {
                for (int i = 0; i < quantity; i++)
                {
                    // For simplicity, we'll add a new instance for each item.
                    // A more complex implementation might stack items.
                    _items.Add(itemToAdd);
                }
                Game1.Log($"Added {quantity}x {itemName} to inventory.");
            }
        }

        public void RemoveItem(string itemName, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var itemToRemove = _items.FirstOrDefault(item => item.Name == itemName);
                if (itemToRemove != null)
                {
                    _items.Remove(itemToRemove);
                }
            }
            Game1.Log($"Removed {quantity}x {itemName} from inventory.");
        }

        public int GetItemCount(string itemName)
        {
            return _items.Count(i => i.Name == itemName);
        }
    }
}
