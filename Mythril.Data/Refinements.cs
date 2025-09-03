using static Mythril.Data.RefinementBuilder''

namespace Mythril.Data;


public readonly record struct Recipe(int InputQuantity, Item OutputItem, int OutputQuantity);

[Singleton]
public class ItemRefinements(CadenceAbilities abilities, Items items) : ISubContent<CadenceAbility, Dictionary<Item, Recipe>>
{
    public Dictionary<Item, Recipe> this[CadenceAbility key] => ByKey[key];

    public Dictionary<CadenceAbility, Dictionary<Item, Recipe>> ByKey { get; } = new()
    {
        { abilities.RefineFire, new Dictionary<Item, Recipe>
        ([
            Input(items.BasicGem).Output(items.FireI, 5),
            Input(items.IronOre).Output(items.FireI, 5),
        ]) },
        { abilities.RefineWood, new Dictionary<Item, Recipe>
        ([
            Input(items.Log).Output(items.Herb, 2),
        ]) },
        { abilities.RefineMixology, new Dictionary<Item, Recipe>
        ([
            Input(items.Herb, 2).Output(items.Potion),
        ]) },
    };
}

public static class RefinementBuilder
{
    public static Dictionary<Item, Recipe> Recipes(params KeyValuePair<Item, Recipe>[] pairs) => new(pairs);
    public static IOutput Input(Item item, int quantity = 1) => new Builder(item, quantity);
    public interface IOutput
    {
        public KeyValuePair<Item, Recipe> Output(Item item, int quantity = 1);
    }
    private class Builder(Item item, int quantity) : IOutput
    {
        private readonly int inputQuantity = quantity;
        private readonly Item inputItem = item;
        public KeyValuePair<Item, Recipe> Output(Item item, int quantity = 1)
        {
            var recipe = new Recipe(inputQuantity, item, quantity);
            return new KeyValuePair<Item, Recipe>(inputItem, recipe);
        }
    }
}