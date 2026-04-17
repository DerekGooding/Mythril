using System.Collections.Immutable;

namespace Mythril.Data;

public interface IGameAction { }

public record AddResourceAction(string ItemName, int Quantity) : IGameAction;
public record SpendResourceAction(string ItemName, int Quantity) : IGameAction;
public record CompleteQuestAction(Quest Quest) : IGameAction;
public record LockQuestAction(Quest Quest) : IGameAction;
public record StartQuestAction(QuestProgress Progress) : IGameAction;
public record CancelQuestAction(QuestProgress Progress) : IGameAction;
public record AssignCadenceAction(string CadenceName, string CharacterName) : IGameAction;
public record UnassignCadenceAction(string CadenceName) : IGameAction;
public record JunctionMagicAction(Character Character, Stat Stat, Item Magic) : IGameAction;
public record UnjunctionAction(Character Character, Stat Stat) : IGameAction;
public record TickAction(double DeltaSeconds) : IGameAction;
public record SkipTimeAction(double Seconds) : IGameAction;
public record UnlockAbilityAction(string AbilityKey) : IGameAction;
public record UnlockCadenceAction(string CadenceName) : IGameAction;
public record ToggleAutoQuestAction(string CharacterName, bool Enabled) : IGameAction;
public record TogglePinAction(string ItemName) : IGameAction;
public record ToggleRecipeStarAction(string RecipeKey) : IGameAction;
public record SetMagicCapacityAction(int Capacity) : IGameAction;
public record ClearInventoryAction() : IGameAction;
public record SetStateAction(GameState NewState) : IGameAction;
public record AddStatBoostAction(string CharacterName, string StatName, int Amount) : IGameAction;
public record UnlockLocationAction(string LocationName) : IGameAction;
public record SetHighlightedPathAction(ImmutableHashSet<string> Path) : IGameAction;
public record ClearHighlightedPathAction() : IGameAction;
public record AddToJournalAction(string TaskName, string CharacterName, string Details, bool WasCancelled = false) : IGameAction;
public record ClearJournalAction() : IGameAction;
public record FinishQuestAction(QuestProgress Progress) : IGameAction;
public record SetActiveTabAction(string TabName) : IGameAction;
public record SetUnseenFlagsAction(bool Cadence, bool Workshop) : IGameAction;
public record SetTestModeAction(bool IsTestMode) : IGameAction;
public record ToggleMiniLogsAction() : IGameAction;
