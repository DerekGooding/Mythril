using Newtonsoft.Json;

namespace Mythril.GameLogic.Materia;

public enum MateriaType
{
    Magic,
    Summon,
    Command,
    Independent,
    Support
}

public abstract class Materia
{
    public string Name { get; set; }
    public string Description { get; set; }
    public MateriaType Type { get; set; }
    public int AP { get; set; }
    public int MaxAP { get; set; }
    public int Level { get; set; }
    public int MaxLevel { get; set; }

    [JsonConstructor]
    protected Materia(string name, string description, MateriaType type, int maxAP, int maxLevel)
    {
        Name = name;
        Description = description;
        Type = type;
        AP = 0;
        MaxAP = maxAP;
        Level = 1;
        MaxLevel = maxLevel;
    }

    protected Materia()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public void AddAP(int amount)
    {
        if (Level >= MaxLevel)
        {
            AP = System.Math.Min(AP + amount, MaxAP);
            return;
        }

        AP += amount;
        while (AP >= MaxAP && Level < MaxLevel)
        {
            AP -= MaxAP;
            Level++;
            Game1.Log($"{Name} leveled up to level {Level}!");
        }

        if (Level >= MaxLevel)
        {
            AP = System.Math.Min(AP, MaxAP);
        }
    }
}
