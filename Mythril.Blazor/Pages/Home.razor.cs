using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mythril.Blazor.Services;
using Mythril.Data;
using System.Timers;

namespace Mythril.Blazor.Pages;

public partial class Home : IDisposable
{
    [Inject] public ResourceManager resourceManager { get; set; } = null!;
    [Inject] public JunctionManager JunctionManager { get; set; } = null!;
    [Inject] public ThemeService themeService { get; set; } = null!;
    [Inject] public NavigationManager navigationManager { get; set; } = null!;
    [Inject] public SnackbarService SnackbarService { get; set; } = null!;
    [Inject] public AuthService AuthService { get; set; } = null!;
    [Inject] public PersistenceService persistenceService { get; set; } = null!;
    [Inject] public IJSRuntime JS { get; set; } = null!;
    [Inject] public VersionService VersionService { get; set; } = null!;
    [Inject] public InventoryService inventoryService { get; set; } = null!;

    private System.Timers.Timer? timer;
    private int saveCounter = 0;
    private int _versionClicks = 0;

    protected override async Task OnInitializedAsync()
    {
        await persistenceService.LoadAsync();
        
        timer = new System.Timers.Timer(100);
        timer.Elapsed += (sender, args) => { _ = Task.Run(OnTimerElapsed); };
        timer.AutoReset = true;
        timer.Enabled = true;

        themeService.OnThemeChanged += StateHasChanged;
        VersionService.OnUpdateAvailable += HandleUpdateAvailable;
        resourceManager.OnItemOverflow += HandleItemOverflow;
    }

    private void HandleItemOverflow(string itemName, int overflowAmount)
    {
        SnackbarService.Show($"Magic Capacity reached! Lost {overflowAmount}x {itemName}.", "warning");
        inventoryService.NotifyOverflow(itemName);
    }

    private async void HandleUpdateAvailable()
    {
        await InvokeAsync(StateHasChanged);
    }

    private void MarkCadencesAsSeen()
    {
        resourceManager.ActiveTab = "cadence";
        if (resourceManager.HasUnseenCadence)
        {
            resourceManager.HasUnseenCadence = false;
            _ = persistenceService.SaveAsync();
        }
    }

    private void MarkWorkshopAsSeen()
    {
        resourceManager.ActiveTab = "workshop";
        if (resourceManager.HasUnseenWorkshop)
        {
            resourceManager.HasUnseenWorkshop = false;
            _ = persistenceService.SaveAsync();
        }
    }

    private async Task ToggleTheme()
    {
        await themeService.ToggleTheme();
    }

    private void ToggleTestMode()
    {
        resourceManager.IsTestMode = !resourceManager.IsTestMode;
    }

    private async Task ResetGame()
    {
        bool confirmed = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to reset all progress? This cannot be undone.");
        if (confirmed)
        {
            await persistenceService.ClearSaveAsync();
            resourceManager.Initialize();
            SnackbarService.Show("Game Reset", "warning");
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task TaskDropped((object item, Character character) args)
    {
        if (args.item is Cadence cadence)
        {
            JunctionManager.AssignCadence(cadence, args.character, resourceManager.UnlockedAbilities);
            SnackbarService.Show($"{args.character.Name} equipped {cadence.Name}", "info");
        }
        else if (args.item is RefinementData refinement)
        {
            if (resourceManager.HasAbility(args.character, refinement.Ability))
            {
                resourceManager.StartQuest(refinement, args.character);
            }
            else
            {
                SnackbarService.Show($"This character does not have a cadence with the {refinement.Ability.Name} ability.", "warning");
            }
        }
        else
        {
            resourceManager.StartQuest(args.item, args.character);
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task UnassignCadence(Cadence cadence)
    {
        JunctionManager.Unassign(cadence, resourceManager.UnlockedAbilities);
        SnackbarService.Show($"Unequipped {cadence.Name}", "info");
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnTimerElapsed()
    {
        resourceManager.Tick(0.1); 
        
        saveCounter++;
        if (saveCounter >= 300) // Save every 30 seconds
        {
            saveCounter = 0;
            await persistenceService.SaveAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (timer != null)
        {
            timer.Enabled = false;
            timer.Dispose();
        }
        themeService.OnThemeChanged -= StateHasChanged;
        VersionService.OnUpdateAvailable -= HandleUpdateAvailable;
    }

    protected async Task OnCompletionAnimationEnd(QuestProgress completedProgress)
    {
        await resourceManager.ReceiveRewards(completedProgress.Item);
        resourceManager.RemoveActiveQuest(completedProgress);
        SnackbarService.Show($"Completed: {completedProgress.Name}", "success");

        // Auto-restart logic (Only for Slot 0)
        bool isRecurring = (completedProgress.Item is QuestData q && q.Type == QuestType.Recurring) || 
                          (completedProgress.Item is RefinementData);

        if (completedProgress.SlotIndex == 0 && isRecurring)
        {
            if (resourceManager.IsAutoQuestEnabled(completedProgress.Character) && resourceManager.CanAutoQuest(completedProgress.Character))
            {
                if (resourceManager.CanAfford(completedProgress.Item, completedProgress.Character))
                {
                    resourceManager.StartQuest(completedProgress.Item, completedProgress.Character, -1.5);
                }
            }
        }
        
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnVersionClick()
    {
        if (AuthService.IsAuthenticated) return;

        _versionClicks++;
        if (_versionClicks >= 3)
        {
            await AuthService.SetDevMode(true);
            _versionClicks = 0;
            SnackbarService.Show("Developer mode unlocked!", "success");
            StateHasChanged();
        }
    }

    private void RefreshPage() => navigationManager.NavigateTo(navigationManager.Uri, forceLoad: true);
}
