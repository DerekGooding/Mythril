namespace Mythril.Data;

public partial class ResourceManager
{
    public record JournalEntry(string TaskName, string CharacterName, string Details, DateTime CompletedAt, bool IsFirstTime = false);
    public List<JournalEntry> Journal { get; set; } = [];
    private readonly HashSet<string> _everPerformedActivities = [];

    public event Action? OnJournalUpdated;

    public void ClearJournal()
    {
        lock(_questLock)
        {
            Journal.Clear();
            _everPerformedActivities.Clear();
        }
        OnJournalUpdated?.Invoke();
    }

    private void AddToJournal(string taskName, string characterName, string details)
    {
        lock(_questLock)
        {
            bool isFirstTime = _everPerformedActivities.Add(taskName);
            Journal.Insert(0, new JournalEntry(taskName, characterName, details, DateTime.Now, isFirstTime));
            if (Journal.Count > 50)
            {
                Journal.RemoveAt(Journal.Count - 1);
            }
        }
        OnJournalUpdated?.Invoke();
    }
}
