using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mythril.Data;

public partial class GameStore
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GameState State { get; private set; } = GameState.Initial;
    public event Action<GameState>? OnStateChanged;
    public event Action<string, int>? OnItemOverflow;
    public event Action? OnJournalUpdated;

    public string ExportState() => JsonSerializer.Serialize(State, _options);
    public void ImportState(string json)
    {
        try
        {
            var newState = JsonSerializer.Deserialize<GameState>(json, _options);
            if (newState != null) Dispatch(new SetStateAction(newState));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to restore state: " + ex.Message);
        }
    }

    public void Dispatch(IGameAction action)
    {
        State = Reduce(State, action, out var overflowItem, out var overflowQty);
        OnStateChanged?.Invoke(State);
        if (overflowItem != null && overflowQty > 0)
        {
            OnItemOverflow?.Invoke(overflowItem, overflowQty);
        }
        if (action is AddToJournalAction || action is ClearJournalAction || action is SetStateAction || action is FinishQuestAction)
        {
            OnJournalUpdated?.Invoke();
        }
    }
}
