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

    public RefinementData? GetRefinement(string abilityName, string inputItemName)
    {
        var kvp = ByKey.FirstOrDefault(x => x.Key.Name == abilityName);
        if (kvp.Key.Name == null) return null;

        var recipeKvp = kvp.Value.FirstOrDefault(x => x.Key.Name == inputItemName);
        if (recipeKvp.Key.Name == null) return null;

        return new RefinementData(kvp.Key, recipeKvp.Key, recipeKvp.Value);
    }
}

public static class RefinementBuilder
{
    // Keeping builder for legacy or manual additions if needed, but not used for JSON loading
    public static Dictionary<Item, Recipe> Recipes(params KeyValuePair<Item, Recipe>[] pairs) => new(pairs);
}
