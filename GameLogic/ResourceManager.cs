namespace Mythril.GameLogic;

public class ResourceManager
{
    public int Gold { get; private set; }
    public int Mana { get; private set; }
    public int Faith { get; private set; }

    public ResourceManager()
    {
        Gold = 0;
        Mana = 0;
        Faith = 0;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        Game1.Log($"Gold: {Gold}");
    }

    public void AddMana(int amount)
    {
        Mana += amount;
        Game1.Log($"Mana: {Mana}");
    }

    public void AddFaith(int amount)
    {
        Faith += amount;
        Game1.Log($"Faith: {Faith}");
    }

    public void Reset()
    {
        Gold = 0;
        Mana = 0;
        Faith = 0;
    }
}
