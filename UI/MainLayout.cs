using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI;

public class MainLayout : Grid
{
    public DropZoneWidget DropZone { get; private set; } = null!;
    public List<CardWidget> CardWidgets { get; }

    private GameManager _gameManager = null!;
    private TaskManager _taskManager = null!; // Added TaskManager
    private VerticalStackPanel _taskProgressPanel = null!; // Panel to hold task progress widgets
    private Game1 _game = null!; // Reference to Game1 instance
    private Desktop _desktop = null!; // Reference to the Myra Desktop
    private Label _goldLabel = null!;
    private Label _manaLabel = null!;
    private Label _faithLabel = null!;
    private HorizontalStackPanel _handPanel = null!; // Reference to the hand panel
    private ResourceManager _resourceManager = null!; // Add ResourceManager field

    public MainLayout(Game1 game, TaskManager taskManager, Desktop desktop, ResourceManager resourceManager) // Constructor now takes Game1, TaskManager, Desktop, and ResourceManager
    {
        _game = game; // Assign Game1 instance
        _desktop = desktop; // Assign Desktop instance
        _resourceManager = resourceManager; // Assign ResourceManager instance
        _gameManager = new GameManager(_resourceManager); // Pass ResourceManager to GameManager
        _taskManager = taskManager; // Assign TaskManager

        // Subscribe to TaskManager events
        _taskManager.OnTaskStarted += OnTaskStarted;
        _taskManager.OnTaskCompleted += OnTaskCompleted;

        DefineGrid();
        InitializeResourcePanel();
        InitializeHandPanel();
        InitializeDropZone();
        InitializeButtonPanel();
        InitializeTaskProgressPanel();

        // Add some CardWidget instances
        CardWidgets = [];
        AddInitialCards();
    }

    private void DefineGrid()
    {
        // Define rows
        RowsProportions.Add(new Proportion(ProportionType.Auto)); // Top row for resources
        RowsProportions.Add(new Proportion(ProportionType.Fill)); // Middle row for hand panel
        RowsProportions.Add(new Proportion(ProportionType.Auto)); // Bottom row for buttons
        RowsProportions.Add(new Proportion(ProportionType.Auto)); // New row for task progress

        // Define columns
        ColumnsProportions.Add(new Proportion(ProportionType.Fill)); // Left column for hand panel
        ColumnsProportions.Add(new Proportion(ProportionType.Auto)); // Middle column for drop zone
        ColumnsProportions.Add(new Proportion(ProportionType.Fill)); // Right column for balance or future use
    }

    private void InitializeResourcePanel()
    {
        // Top Row: Resource Display
        var resourcePanel = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Spacing = 20
        };

        var goldPanel = new HorizontalStackPanel { Spacing = 5 };
        goldPanel.Widgets.Add(new Label { Text = "Gold:" });
        _goldLabel = new Label { Text = "0" };
        goldPanel.Widgets.Add(_goldLabel);
        resourcePanel.Widgets.Add(goldPanel);

        var manaPanel = new HorizontalStackPanel { Spacing = 5 };
        manaPanel.Widgets.Add(new Label { Text = "Mana:" });
        _manaLabel = new Label { Text = "0" };
        manaPanel.Widgets.Add(_manaLabel);
        resourcePanel.Widgets.Add(manaPanel);

        var faithPanel = new HorizontalStackPanel { Spacing = 5 };
        faithPanel.Widgets.Add(new Label { Text = "Faith:" });
        _faithLabel = new Label { Text = "0" };
        faithPanel.Widgets.Add(_faithLabel);
        resourcePanel.Widgets.Add(faithPanel);

        SetRow(resourcePanel, 0);
        Widgets.Add(resourcePanel);
    }

    private void InitializeHandPanel()
    {
        // Middle Row: Hand Panel
        _handPanel = new HorizontalStackPanel // Changed from Panel to HorizontalStackPanel to arrange cards
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Spacing = 10 // Add spacing between cards
        };
        SetRow(_handPanel, 1);
        SetColumn(_handPanel, 0);
        Widgets.Add(_handPanel);
    }

    private void InitializeDropZone()
    {
        // Add a DropZoneWidget
        DropZone = new DropZoneWidget(_taskManager)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        SetRow(DropZone, 1);
        SetColumn(DropZone, 1); // Place it in the middle column
        Widgets.Add(DropZone);
    }

    private void InitializeButtonPanel()
    {
        // Bottom Row: Action Buttons
        var buttonPanel = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10
        };
        var collectButton = new Button { Content = new Label { Text = "Collect" } };
        collectButton.Click += (s, a) => _gameManager.CollectRewards();
        buttonPanel.Widgets.Add(collectButton);

        var nextDayButton = new Button { Content = new Label { Text = "Next Day" } };
        nextDayButton.Click += (s, a) => _gameManager.AdvanceTick();
        buttonPanel.Widgets.Add(nextDayButton);

        var fullscreenButton = new Button { Content = new Label { Text = "Fullscreen" } };
        fullscreenButton.Click += (s, a) => _game.ToggleFullscreen();
        buttonPanel.Widgets.Add(fullscreenButton);

        var settingsButton = new Button { Content = new Label { Text = "Settings" } };
        settingsButton.Click += (s, a) =>
        {
            var settingsDialog = new SettingsDialog();
            settingsDialog.ShowModal(_desktop);
        };
        buttonPanel.Widgets.Add(settingsButton);

        var pauseButton = new Button { Content = new Label { Text = "Pause" } };
        pauseButton.Click += (s, a) => _game.TogglePause(); // Will implement TogglePause in Game1
        buttonPanel.Widgets.Add(pauseButton);

        SetRow(buttonPanel, 2);
        Widgets.Add(buttonPanel);
    }

    private void InitializeTaskProgressPanel()
    {
        // Task Progress Panel
        _taskProgressPanel = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Spacing = 5
        };
        SetRow(_taskProgressPanel, 3); // Place in the new row
        Widgets.Add(_taskProgressPanel);
    }

    private void AddInitialCards()
    {
        var card1Data = new CardData { Id = "card1", Title = "Forest Foraging", Description = "Gather herbs and berries from the forest.", DurationSeconds = 60, RewardValue = 10 };
        var card2Data = new CardData { Id = "card2", Title = "Mine Exploration", Description = "Explore the abandoned mine for valuable ores.", DurationSeconds = 120, RewardValue = 25 };
        var card3Data = new CardData { Id = "card3", Title = "River Fishing", Description = "Catch fresh fish from the river.", DurationSeconds = 90, RewardValue = 15 };
        var card4Data = new CardData { Id = "card4", Title = "Pray", Description = "Generate Faith through prayer.", DurationSeconds = 30, RewardValue = 5 };
        var card5Data = new CardData { Id = "card5", Title = "Build Shrine", Description = "Consume Faith and Gold to build a shrine.", DurationSeconds = 180, RewardValue = 100 };


        var card1 = new CardWidget(card1Data, DropZone);
        var card2 = new CardWidget(card2Data, DropZone);
        var card3 = new CardWidget(card3Data, DropZone);
        var card4 = new CardWidget(card4Data, DropZone);
        var card5 = new CardWidget(card5Data, DropZone);

        _handPanel.Widgets.Add(card1);
        _handPanel.Widgets.Add(card2);
        _handPanel.Widgets.Add(card3);
        _handPanel.Widgets.Add(card4);
        _handPanel.Widgets.Add(card5);

        CardWidgets.Add(card1);
        CardWidgets.Add(card2);
        CardWidgets.Add(card3);
        CardWidgets.Add(card4);
        CardWidgets.Add(card5);
    }

    public void HandleCardDrop(CardWidget draggedCard)
    {
        // Check if the dragged card is over the drop zone
        if (draggedCard.Bounds.Intersects(DropZone.Bounds))
        {
            DropZone.HandleDrop(draggedCard);
            // Optionally, remove the card from the hand panel after dropping
            // _handPanel.Widgets.Remove(draggedCard);
        }
    }

    public void UpdateResources(ResourceManager resourceManager)
    {
        _goldLabel.Text = resourceManager.Gold.ToString();
        _manaLabel.Text = resourceManager.Mana.ToString();
        _faithLabel.Text = resourceManager.Faith.ToString();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var widget in _taskProgressPanel.Widgets)
        {
            if (widget is TaskProgressWidget taskProgressWidget)
            {
                taskProgressWidget.UpdateProgress();
            }
        }
    }

    public void ResetCards()
    {
        _handPanel.Widgets.Clear();
        CardWidgets.Clear();
        AddInitialCards();
    }

    private void OnTaskStarted(TaskProgress task)
    {
        var widget = new TaskProgressWidget(task);
        _taskProgressPanel.Widgets.Add(widget);
    }

    private void OnTaskCompleted(TaskProgress task)
    {
        // Find and remove the completed task's widget
        var widgetToRemove = _taskProgressPanel.Widgets.OfType<TaskProgressWidget>()
                                .FirstOrDefault(w => w.TaskProgress == task); // Assuming TaskProgressWidget has a public TaskProgress property
        if (widgetToRemove != null)
        {
            _taskProgressPanel.Widgets.Remove(widgetToRemove);
        }
    }
}
