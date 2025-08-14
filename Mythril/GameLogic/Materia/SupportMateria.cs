using Newtonsoft.Json;

namespace Mythril.GameLogic.Materia;

public class SupportMateria : Materia
{
    // The paired materia will be handled by the equipment system
    public Materia? PairedMateria { get; set; }

    [JsonConstructor]
    public SupportMateria(string name, string description, int maxAP, int maxLevel)
        : base(name, description, MateriaType.Support, maxAP, maxLevel)
    {
    }

    public SupportMateria() { }
}
