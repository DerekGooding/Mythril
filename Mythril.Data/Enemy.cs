namespace Mythril.Data;

public class Enemy(string name, string jobName, string zone) : Character(name)
{
    public string Zone { get; set; } = zone;
}
