using Microsoft.AspNetCore.Components;
using Mythril.Data;
using Mythril.Blazor.Services;

namespace Mythril.Blazor.Components;

public partial class CharacterDisplay : IDisposable
{
    [Inject] public Stats statsContent { get; set; } = null!;
    [Inject] public ResourceManager resourceManager { get; set; } = null!;
    [Inject] public JunctionManager JunctionManager { get; set; } = null!;
    [Inject] public DragDropService DragDropService { get; set; } = null!;
    [Inject] public SnackbarService SnackbarService { get; set; } = null!;

    [Parameter] public Character Character { get; set; }
    [Parameter] public IEnumerable<QuestProgress> QuestProgresses { get; set; } = [];
    [Parameter] public EventCallback<object> OnQuestDrop { get; set; }
    [Parameter] public Func<object, bool>? Accepts { get; set; }
    [Parameter] public EventCallback<QuestProgress> OnCompletionAnimationEnd { get; set; }
    [Parameter] public EventCallback<Cadence> OnUnequip { get; set; }

    private bool _showJunctionMenu = false;
    private bool _isRemovalMode = false;

    protected override void OnInitialized()
    {
        DragDropService.OnHoverChanged += HandleHoverChanged;
    }

    public void Dispose()
    {
        DragDropService.OnHoverChanged -= HandleHoverChanged;
    }

    private void HandleHoverChanged()
    {
        InvokeAsync(StateHasChanged);
    }
}
