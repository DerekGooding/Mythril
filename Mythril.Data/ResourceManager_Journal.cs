using System.Collections.Immutable;

namespace Mythril.Data;

public partial class ResourceManager
{
    public ImmutableList<JournalEntry> Journal => _gameStore.State.Journal;
    public ImmutableHashSet<string> EverPerformedActivities => _gameStore.State.EverPerformedActivities;

    public event Action? OnJournalUpdated;

    public void ClearJournal()
    {
        _gameStore.Dispatch(new ClearJournalAction());
        OnJournalUpdated?.Invoke();
    }

    public ImmutableDictionary<string, ImmutableList<string>> CharacterMiniLogs => _gameStore.State.CharacterMiniLogs;

    public void AddToJournal(string taskName, string characterName, string details)
    {
        _gameStore.Dispatch(new AddToJournalAction(taskName, characterName, details));
        OnJournalUpdated?.Invoke();
    }
}
