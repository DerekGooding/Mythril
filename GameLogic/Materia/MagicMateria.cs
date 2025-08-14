using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mythril.GameLogic.Materia
{
    public class MagicMateria : Materia
    {
        public List<string> Spells { get; set; }

        [JsonConstructor]
        public MagicMateria(string name, string description, int maxAP, int maxLevel, List<string> spells)
            : base(name, description, MateriaType.Magic, maxAP, maxLevel)
        {
            Spells = spells;
        }

        public MagicMateria()
        {
            Spells = new List<string>();
        }
    }
}
