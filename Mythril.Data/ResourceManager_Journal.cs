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

    public Dictionary<string, Queue<string>> CharacterMiniLogs = [];

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

            // Update mini-log
            if (!CharacterMiniLogs.ContainsKey(characterName))
                CharacterMiniLogs[characterName] = new Queue<string>();
            
            var log = CharacterMiniLogs[characterName];
            log.Enqueue(taskName);
            if (log.Count > 3) log.Dequeue();
        }
        OnJournalUpdated?.Invoke();
    }
}
