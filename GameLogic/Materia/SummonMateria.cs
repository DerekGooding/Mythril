using Newtonsoft.Json;

namespace Mythril.GameLogic.Materia
{
    public class SummonMateria : Materia
    {
        public string SummonName { get; set; }

        [JsonConstructor]
        public SummonMateria(string name, string description, int maxAP, int maxLevel, string summonName)
            : base(name, description, MateriaType.Summon, maxAP, maxLevel)
        {
            SummonName = summonName;
        }

        public SummonMateria()
        {
            SummonName = string.Empty;
        }
    }
}
