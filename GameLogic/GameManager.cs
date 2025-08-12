namespace Mythril.GameLogic;

public class GameManager(ResourceManager resourceManager)
{
    private readonly ResourceManager _resourceManager = resourceManager;

    public event Action? OnGameOver;

    public void CollectRewards() => Game1.Log("Collect button clicked!");

    public void AdvanceTick()
    {
        Game1.Log("Next Day button clicked!");
        // Simulate resource depletion for testing game over
        _resourceManager.AddGold(-5); // Deduct some gold
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (_resourceManager.Gold <= 0)
        {
            OnGameOver?.Invoke();
        }
    }
}
