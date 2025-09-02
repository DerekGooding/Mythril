namespace Mythril.Data;


public readonly record struct ItemAmount(Item Item, int Quantity = 1);

public readonly record struct Recipe(CadenceAbility Ability, int OutputQuantity, ItemAmount[] Cost);

[Singleton]
public partial class ItemRefinements(CadenceAbilities abilities, Items items) : ISubContent<Item, Recipe[]>
{
    public Recipe[] this[Item key] => ByKey[key];

    public Dictionary<Item, Recipe[]> ByKey { get; } = new()
    {
        { items.FireI, [ new(abilities.RefineFire, 5, [ new (items.BasicGem)]) ] },
    };
}