using Mythril.Data;

namespace Mythril.Tests;

public static class SandboxContent
{
    // Items
    public const string Gold = "Gold";
    public const string Scrap = "Scrap";
    public const string Potion = "Potion";
    public const string Wood = "Wood";
    public const string IronOre = "Iron Ore";
    public const string Iron = "Iron";
    public const string AncientBark = "Ancient Bark";
    public const string BasicGem = "Basic Gem";
    public const string FireI = "Fire I";
    public const string Slime = "Slime";
    public const string Web = "Web";
    public const string Log = "Log";

    // Quests
    public const string Prologue = "Prologue";
    public const string Tutorial = "Tutorial";
    public const string BuyPotion = "Buy Potion";
    public const string ChopWood = "Chop Wood";
    public const string RekindlingTheSpark = "Rekindling the Spark";
    public const string HuntGoblins = "Hunt Goblins";
    public const string HuntBats = "Hunt Bats";
    public const string HuntSpiders = "Hunt Spiders";

    // Stats
    public const string Strength = "Strength";
    public const string Vitality = "Vitality";
    public const string Magic = "Magic";
    public const string Speed = "Speed";

    // Cadences & Abilities
    public const string Recruit = "Recruit";
    public const string Apprentice = "Apprentice";
    public const string Arcanist = "Arcanist";
    public const string Sentinel = "The Sentinel";
    public const string Weaver = "Mythril Weaver";
    public const string Scholar = "Scholar";
    public const string Student = "Student";
    public const string Geologist = "Geologist";
    public const string Slayer = "Slayer";
    public const string TideCaller = "Tide-Caller";

    public const string AutoQuestI = "AutoQuest I";
    public const string AutoQuestII = "AutoQuest II";
    public const string RefineScrap = "Refine Scrap";
    public const string RefineFire = "Refine Fire";
    public const string LogisticsI = "Logistics I";
    public const string LogisticsII = "Logistics II";
    public const string JStr = "J-Str";
    public const string JSpd = "J-Speed";
    public const string JVit = "J-Vit";
    public const string JMag = "J-Magic";
    public const string MagicPocketI = "Magic Pocket I";
    public const string MagicPocketII = "Magic Pocket II";

    public static void Load()
    {
        // 1. Items
        var items = new List<Item>
        {
            new(Gold, "Currency", ItemType.Currency),
            new(Scrap, "Material", ItemType.Material),
            new(Potion, "Consumable", ItemType.Consumable),
            new(Wood, "Material", ItemType.Material),
            new(IronOre, "Material", ItemType.Material),
            new(Iron, "Material", ItemType.Material),
            new(AncientBark, "Material", ItemType.Material),
            new(BasicGem, "Material", ItemType.Material),
            new(FireI, "Fire Spell", ItemType.Spell),
            new(Slime, "Material", ItemType.Material),
            new(Web, "Material", ItemType.Material),
            new(Log, "Material", ItemType.Material)
        };
        ContentHost.GetContent<Items>().Load(items);

        // 2. Stats
        var stats = new List<Stat>
        {
            new(Strength, "Physical power"),
            new(Vitality, "Health and defense"),
            new(Magic, "Mystical power"),
            new(Speed, "Agility")
        };
        ContentHost.GetContent<Stats>().Load(stats);

        // 3. Abilities
        var abilities = new List<CadenceAbility>
        {
            new(AutoQuestI, "Unlocks Auto-Questing") { Effects = [new EffectDefinition(EffectType.AutoQuest, 1)] },
            new(AutoQuestII, "Advanced Auto-Questing") { Effects = [new EffectDefinition(EffectType.AutoQuest, 2)] },
            new(RefineScrap, "Can refine scrap into gold") { Effects = [] },
            new(RefineFire, "Can refine gems into fire") { Effects = [] },
            new(LogisticsI, "Increased task limit") { Effects = [new EffectDefinition(EffectType.Logistics, 1)] },
            new(LogisticsII, "Further increased task limit") { Effects = [new EffectDefinition(EffectType.Logistics, 2)] },
            new(JStr, "Strength Junction") { Effects = [new EffectDefinition(EffectType.StatBoost, 1, Strength)] },
            new(JSpd, "Speed Junction") { Effects = [new EffectDefinition(EffectType.StatBoost, 1, Speed)] },
            new(JVit, "Vitality Junction") { Effects = [new EffectDefinition(EffectType.StatBoost, 1, Vitality)] },
            new(JMag, "Magic Junction") { Effects = [new EffectDefinition(EffectType.StatBoost, 1, Magic)] },
            new(MagicPocketI, "Bag space") { Effects = [new EffectDefinition(EffectType.MagicCapacity, 60)] },
            new(MagicPocketII, "More bag space") { Effects = [new EffectDefinition(EffectType.MagicCapacity, 100)] }
        };
        ContentHost.GetContent<CadenceAbilities>().Load(abilities);

        // 4. Cadences
        var recruitUnlocks = new List<CadenceUnlock>
        {
            new(Recruit, abilities.First(a => a.Name == AutoQuestI), [], Magic),
            new(Recruit, abilities.First(a => a.Name == RefineScrap), [], Strength),
            new(Recruit, abilities.First(a => a.Name == JStr), [], Strength)
        };
        var apprenticeUnlocks = new List<CadenceUnlock>
        {
            new(Apprentice, abilities.First(a => a.Name == AutoQuestI), [], Magic),
            new(Apprentice, abilities.First(a => a.Name == RefineFire), [], Magic)
        };
        var arcanistUnlocks = new List<CadenceUnlock>
        {
            new(Arcanist, abilities.First(a => a.Name == JStr), [], Magic),
            new(Arcanist, abilities.First(a => a.Name == JMag), [], Magic),
            new(Arcanist, abilities.First(a => a.Name == MagicPocketI), [], Magic)
        };
        var sentinelUnlocks = new List<CadenceUnlock>
        {
            new(Sentinel, abilities.First(a => a.Name == JStr), [], Vitality),
            new(Sentinel, abilities.First(a => a.Name == JVit), [], Vitality),
            new(Sentinel, abilities.First(a => a.Name == MagicPocketII), [], Vitality)
        };
        var weaverUnlocks = new List<CadenceUnlock>
        {
            new(Weaver, abilities.First(a => a.Name == LogisticsI), [], Vitality),
            new(Weaver, abilities.First(a => a.Name == JVit), [], Vitality)
        };
        var scholarUnlocks = new List<CadenceUnlock>
        {
            new(Scholar, abilities.First(a => a.Name == LogisticsII), [], Magic),
            new(Scholar, abilities.First(a => a.Name == AutoQuestII), [], Magic)
        };
        var studentUnlocks = new List<CadenceUnlock>
        {
            new(Student, abilities.First(a => a.Name == AutoQuestI), [], Magic),
            new(Student, abilities.First(a => a.Name == RefineFire), [], Magic),
            new(Student, abilities.First(a => a.Name == JSpd), [], Speed)
        };

        var cadences = new List<Cadence>
        {
            new(Recruit, "Entry level cadence", [.. recruitUnlocks]),
            new(Apprentice, "Apprentice level cadence", [.. apprenticeUnlocks]),
            new(Arcanist, "Magic user", [.. arcanistUnlocks]),
            new(Sentinel, "Defender", [.. sentinelUnlocks]),
            new(Weaver, "Advanced craft cadence", [.. weaverUnlocks]),
            new(Scholar, "Master of systems", [.. scholarUnlocks]),
            new(Student, "Eager learner", [.. studentUnlocks]),
            new(Geologist, "Rock expert", []),
            new(Slayer, "Monster hunter", []),
            new(TideCaller, "Ocean mage", [])
        };
        ContentHost.GetContent<Cadences>().Load(cadences);

        // 5. Quests
        var qs = new List<Quest>
        {
            new(Prologue, "The beginning"),
            new(Tutorial, "Learning the ropes"),
            new(BuyPotion, "Need a drink"),
            new(ChopWood, "Need wood"),
            new(RekindlingTheSpark, "Fix the forge"),
            new(HuntGoblins, "Clear out goblins"),
            new(HuntBats, "Clear mine entrance"),
            new(HuntSpiders, "Clear deeper webs")
        };
        ContentHost.GetContent<Quests>().Load(qs);

        // 6. Quest Details
#pragma warning disable TND001
        var details = new Dictionary<Quest, QuestDetail>();
        var qMap = qs.ToDictionary(q => q.Name);
        var iMap = items.ToDictionary(i => i.Name);
#pragma warning restore TND001

        details[qMap[Prologue]] = new QuestDetail(5, [], [], QuestType.Single, Vitality);
        details[qMap[Tutorial]] = new QuestDetail(10, [], [new ItemQuantity(iMap[Scrap], 5)], QuestType.Single, Vitality);
        // Make BuyPotion Recurring so multiple can be started for SlotAllocation test
        details[qMap[BuyPotion]] = new QuestDetail(1, [new ItemQuantity(iMap[Gold], 100)], [new ItemQuantity(iMap[Potion], 1)], QuestType.Recurring, Magic);
        details[qMap[ChopWood]] = new QuestDetail(20, [], [new ItemQuantity(iMap[Wood], 2)], QuestType.Recurring, Strength, new Dictionary<string, int> { { Strength, 10 } });
        details[qMap[RekindlingTheSpark]] = new QuestDetail(60, [new ItemQuantity(iMap[IronOre], 10)], [], QuestType.Single, Magic);
        details[qMap[HuntGoblins]] = new QuestDetail(60, [], [new ItemQuantity(iMap[Slime], 1)], QuestType.Recurring, Strength);
        details[qMap[HuntBats]] = new QuestDetail(45, [], [], QuestType.Recurring, Speed);
        details[qMap[HuntSpiders]] = new QuestDetail(75, [], [], QuestType.Recurring, Vitality);

        ContentHost.GetContent<QuestDetails>().Load(details);

        // 7. Quest Unlocks (Dependencies)
        var unlocks = new Dictionary<Quest, Quest[]>
        {
            { qMap[Tutorial], [qMap[Prologue]] },
            { qMap[BuyPotion], [qMap[Tutorial]] }
        };
        ContentHost.GetContent<QuestUnlocks>().Load(unlocks);

        // 8. Locations
        var locations = new List<Location>
        {
            new("Starting Area", [qMap[Prologue], qMap[Tutorial], qMap[BuyPotion]]),
            new("Forest", [qMap[ChopWood]], Tutorial)
        };
        ContentHost.GetContent<Locations>().Load(locations);

        // 9. Quest to Cadence Unlocks
        var qToCad = new Dictionary<Quest, Cadence[]>
        {
            { qMap[Prologue], [cadences.First(c => c.Name == Recruit)] }
        };
        ContentHost.GetContent<QuestToCadenceUnlocks>().Load(qToCad);

        // 10. Item Refinements
        var refinementDict = new Dictionary<CadenceAbility, (string PrimaryStat, Dictionary<Item, Recipe> Recipes)>();
        var refineScrapAb = abilities.First(a => a.Name == RefineScrap);
        refinementDict[refineScrapAb] = (Strength, new Dictionary<Item, Recipe>
        {
            { iMap[Scrap], new Recipe(5, iMap[Gold], 10) }
        });

        var refineFireAb = abilities.First(a => a.Name == RefineFire);
        refinementDict[refineFireAb] = (Magic, new Dictionary<Item, Recipe>
        {
            { iMap[BasicGem], new Recipe(1, iMap[FireI], 10) }
        });
        ContentHost.GetContent<ItemRefinements>().Load(refinementDict);

        // 11. Stat Augments (Empty for now)
        ContentHost.GetContent<StatAugments>().Load([]);

        // 12. Ability Augments
        var abAugDict = new Dictionary<CadenceAbility, Stat>
        {
            { abilities.First(a => a.Name == AutoQuestI), stats.First(s => s.Name == Magic) },
            { abilities.First(a => a.Name == RefineScrap), stats.First(s => s.Name == Strength) }
        };
        ContentHost.GetContent<AbilityAugments>().Load(abAugDict);
    }
}