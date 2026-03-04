namespace Mythril.Data;

public partial class ResourceManager
{
    public record JournalEntry(string TaskName, string CharacterName, string Details, DateTime CompletedAt);
    public List<JournalEntry> Journal { get; set; } = [];

    private void AddToJournal(string taskName, string characterName, string details)
    {
        lock(_questLock)
        {
            Journal.Insert(0, new JournalEntry(taskName, characterName, details, DateTime.Now));
            if (Journal.Count > 50)
            {
                Journal.RemoveAt(Journal.Count - 1);
            }
        }
    }
}
