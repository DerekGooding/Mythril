using Newtonsoft.Json;

namespace Mythril.Data.Materia;

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
}
