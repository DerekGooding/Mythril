using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
    private bool _showHelp = false;

    private async Task HandleGlobalKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "1") await SwitchTab("hand-tab");
        if (e.Key == "2") await SwitchTab("cadence-tab");
        if (e.Key == "3") await SwitchTab("workshop-tab");
        if (e.Key == "?" || e.Key == "/") _showHelp = !_showHelp;
    }

    private async Task SwitchTab(string tabId)
    {
        await JS.InvokeVoidAsync("eval", $"document.getElementById('{tabId}').click()");
    }

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
        resourceManager.MarkSeen("workshop");
    }

    private void MarkLocationsAsSeen()
    {
        resourceManager.ActiveTab = "hand";
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
        await resourceManager.ReceiveRewards(completedProgress);
        SnackbarService.Show($"Completed: {completedProgress.Name}", "success");
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnVersionClick()
    {
        _versionClicks++;
        if (_versionClicks >= 3)
        {
            bool newState = !AuthService.IsAuthenticated;
            await AuthService.SetDevMode(newState);
            _versionClicks = 0;
            SnackbarService.Show(newState ? "Developer mode unlocked!" : "Developer mode disabled.", newState ? "success" : "info");
            StateHasChanged();
        }
    }

    private void RefreshPage() => navigationManager.NavigateTo(navigationManager.Uri, forceLoad: true);
}
