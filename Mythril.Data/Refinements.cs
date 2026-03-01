namespace Mythril.Data;

[Singleton]
public class ItemRefinements : ISubContent<CadenceAbility, Dictionary<Item, Recipe>>
{
    public Dictionary<Item, Recipe> this[CadenceAbility key] => ByKey.TryGetValue(key, out var item) ? item : [];

    public Dictionary<CadenceAbility, Dictionary<Item, Recipe>> ByKey { get; } = [];

    public void Load(Dictionary<CadenceAbility, Dictionary<Item, Recipe>> data)
    {
        ByKey.Clear();
        foreach (var kvp in data) ByKey[kvp.Key] = kvp.Value;
    }
}

public static class RefinementBuilder
{
    // Keeping builder for legacy or manual additions if needed, but not used for JSON loading
    public static Dictionary<Item, Recipe> Recipes(params KeyValuePair<Item, Recipe>[] pairs) => new(pairs);
}
