namespace Mythril.Data;

public partial class ResourceManager
{
    public void RemoveActiveQuest(QuestProgress progress)
    {
        lock(_questLock)
        {
            ActiveQuests.Remove(progress);
        }
    }
}
