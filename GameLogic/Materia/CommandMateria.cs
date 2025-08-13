using Newtonsoft.Json;

namespace Mythril.GameLogic.Materia
{
    public class CommandMateria : Materia
    {
        public string CommandName { get; set; }

        [JsonConstructor]
        public CommandMateria(string name, string description, int maxAP, int maxLevel, string commandName)
            : base(name, description, MateriaType.Command, maxAP, maxLevel)
        {
            CommandName = commandName;
        }

        public CommandMateria()
        {
            CommandName = string.Empty;
        }
    }
}
