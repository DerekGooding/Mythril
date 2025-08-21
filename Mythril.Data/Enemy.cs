namespace Mythril.Data;

public class Enemy(string name, string jobName, string zone) : Character(name, jobName)
{
    public string Zone { get; set; } = zone;
}
