using Newtonsoft.Json;

namespace Mythril.GameLogic.Materia
{
    public enum IndependentEffectType
    {
        HP_Plus,
        MP_Plus,
        Speed_Plus,
        Magic_Plus,
        Luck_Plus,
        EXP_Plus,
        Gil_Plus,
        Counter_Attack,
        Cover,
        Long_Range,
        Pre_Emptive
    }

    public class IndependentMateria : Materia
    {
        public IndependentEffectType EffectType { get; set; }
        public int EffectValue { get; set; }

        [JsonConstructor]
        public IndependentMateria(string name, string description, int maxAP, int maxLevel, IndependentEffectType effectType, int effectValue)
            : base(name, description, MateriaType.Independent, maxAP, maxLevel)
        {
            EffectType = effectType;
            EffectValue = effectValue;
        }

        public IndependentMateria()
        {
        }
    }
}
