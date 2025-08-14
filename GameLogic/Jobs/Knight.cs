using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mythril.GameLogic.Jobs
{
    public class Knight : Job
    {
        [JsonConstructor]
        public Knight(string name, string description, List<string> abilities)
            : base(name, description, abilities)
        {
        }

        public Knight() : base() { }
    }
}
