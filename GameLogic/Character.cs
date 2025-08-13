namespace Mythril.GameLogic
{
    public class Character
    {
        public string Name { get; set; }
        public string Job { get; set; }

        public Character(string name, string job)
        {
            Name = name;
            Job = job;
        }
    }
}
