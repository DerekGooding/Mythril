namespace Mythril.Data;

[Unique]
public readonly partial record struct CadenceAbility(string Name, string Description) : INamed;


[Singleton]
public partial class CadenceAbilities : IContent<CadenceAbility>
{
    public CadenceAbility[] All { get; } =
    [
        new ("AutoQuest I", "Allows a Quest to be looped indefinitely"),
        new ("Refine Fire", "Refine items into fire magic"),
        new ("Refine Ice", "Refine items into ice magic"),

        new ("Refine Wood", "The first crafting refinement. Turns materials into other materials or consumables."),
        new ("Refine Scrap", "A refining ability that allows breaking down complex materials into simplier parts."),
        new ("Refine Mixology", "Allows the creation of potions and elixirs from herbs and other ingredients."),


        new ("Augment Strength", "Augment strength stat with magic"),
        new ("Augment Health", "Augment health stat with magic"),
        new ("Augment Vitality", "Augment vitality stat with magic"),
        new ("Augment Speed", "Augment speed stat with magic"),
        new ("Augment Magic", "Augment magic stat with magic"),
        new ("Augment Spirit", "Augment spirit stat with magic"),
        new ("Augment Luck", "Augment luck stat with magic"),
        new ("Augment Hit", "Augment hit stat with magic"),
        new ("Augment Evade", "Augment evade stat with magic"),
    ];
}
