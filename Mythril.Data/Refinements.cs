namespace Mythril.Data;

[Singleton]
public class ItemRefinements : ISubContent<CadenceAbility, (string PrimaryStat, Dictionary<Item, Recipe> Recipes)>
{
    public (string PrimaryStat, Dictionary<Item, Recipe> Recipes) this[CadenceAbility key] => ByKey.TryGetValue(key, out var item) ? item : ("Strength", []);

    public Dictionary<CadenceAbility, (string PrimaryStat, Dictionary<Item, Recipe> Recipes)> ByKey { get; } = [];

    public void Load(Dictionary<CadenceAbility, (string PrimaryStat, Dictionary<Item, Recipe> Recipes)> data)
    {
        ByKey.Clear();
        foreach (var kvp in data) ByKey[kvp.Key] = kvp.Value;
    }

    public RefinementData? GetRefinement(string abilityName, string inputItemName)
    {
        var kvp = ByKey.FirstOrDefault(x => x.Key.Name == abilityName);
        if (kvp.Key.Name == null) return null;

        var recipeKvp = kvp.Value.Recipes.FirstOrDefault(x => x.Key.Name == inputItemName);
        if (recipeKvp.Key.Name == null) return null;

        return new RefinementData(kvp.Key, recipeKvp.Key, recipeKvp.Value, kvp.Value.PrimaryStat);
    }
}