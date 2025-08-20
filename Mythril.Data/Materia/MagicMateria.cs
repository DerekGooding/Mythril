using Newtonsoft.Json;

namespace Mythril.Data.Materia;

public class MagicMateria : Materia
{
    public List<string> Spells { get; set; }

    [JsonConstructor]
    public MagicMateria(string name, string description, int maxAP, int maxLevel, List<string> spells)
        : base(name, description, MateriaType.Magic, maxAP, maxLevel) => Spells = spells;

    public MagicMateria() => Spells = [];
}
