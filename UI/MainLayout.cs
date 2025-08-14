using System;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Mythril.GameLogic;
using Mythril.GameLogic.Combat;

namespace Mythril.UI;

public class MainLayout : Grid
{
    public DropZoneWidget DropZone { get; private set; } = null!;
    public List<CardWidget> CardWidgets { get; }

    private GameManager _gameManager = null!;
    private PartyManager _partyManager = null!;
    private TaskManager _taskManager = null!; // Added TaskManager
    private Game1 _game = null!; // Reference to Game1 instance
    private Desktop _desktop = null!; // Reference to the Myra Desktop
    private Button _logButton = null!;
    private Button _progressButton = null!;
    private Label _goldLabel = null!;
    private Label _manaLabel = null!;
    private Label _faithLabel = null!;
    private HorizontalStackPanel _handPanel = null!; // Reference to the hand panel
    private ResourceManager _resourceManager = null!; // Add ResourceManager field
    private SoundManager _soundManager = null!; // Add SoundManager field

    public MainLayout(Game1 game, TaskManager taskManager, Desktop desktop, ResourceManager resourceManager, SoundManager soundManager) // Constructor now takes Game1, TaskManager, Desktop, ResourceManager, and SoundManager
    {
        _game = game; // Assign Game1 instance
        _desktop = desktop; // Assign Desktop instance
        _resourceManager = resourceManager; // Assign ResourceManager instance
        _soundManager = soundManager; // Assign SoundManager instance
        _game = game; // Assign Game1 instance
        _desktop = desktop; // Assign Desktop instance
        _resourceManager = resourceManager; // Assign ResourceManager instance
        _gameManager = new GameManager(_resourceManager); // Pass ResourceManager to GameManager
        _partyManager = new PartyManager(_resourceManager);
        _taskManager = taskManager; // Assign TaskManager

        DefineGrid();
        InitializeResourcePanel();
        InitializeHandPanel();
        InitializeDropZone();
        InitializeButtonPanel();
        InitializePartyPanel();

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
            Spacing = 20, // Add spacing between cards
            Padding = new Thickness(10)
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
            Spacing = 20
        };
        var collectButton = new Button { Content = new Label { Text = "Collect" } };
        collectButton.Click += (s, a) => { _gameManager.CollectRewards(); _soundManager.PlaySound("button-click"); };
        buttonPanel.Widgets.Add(collectButton);

        var nextDayButton = new Button { Content = new Label { Text = "Next Day" } };
        nextDayButton.Click += (s, a) => { _gameManager.AdvanceTick(); _soundManager.PlaySound("button-click"); };
        buttonPanel.Widgets.Add(nextDayButton);

        var fullscreenButton = new Button { Content = new Label { Text = "Fullscreen" } };
        fullscreenButton.Click += (s, a) => { _game.ToggleFullscreen(); _soundManager.PlaySound("button-click"); };
        buttonPanel.Widgets.Add(fullscreenButton);

        var settingsButton = new Button { Content = new Label { Text = "Settings" } };
        settingsButton.Click += (s, a) =>
        {
            _soundManager.PlaySound("button-click");
            var settingsDialog = new SettingsDialog();
            settingsDialog.ShowModal(_desktop);
        };
        buttonPanel.Widgets.Add(settingsButton);

        var materiaButton = new Button { Content = new Label { Text = "Materia" } };
        materiaButton.Click += (s, a) =>
        {
            _soundManager.PlaySound("button-click");
            var materiaScreen = new MateriaScreen(_resourceManager);
            materiaScreen.ShowModal(_desktop);
        };
        buttonPanel.Widgets.Add(materiaButton);

        var jobButton = new Button { Content = new Label { Text = "Jobs" } };
        jobButton.Click += (s, a) =>
        {
            _soundManager.PlaySound("button-click");
            var jobScreen = new JobScreen(_resourceManager);
            jobScreen.ShowModal(_desktop);
        };
        buttonPanel.Widgets.Add(jobButton);

        var shopButton = new Button { Content = new Label { Text = "Shop" } };
        shopButton.Click += (s, a) =>
        {
            _soundManager.PlaySound("button-click");
            var shopScreen = new ShopScreen();
            shopScreen.ShowModal(_desktop);
        };
        buttonPanel.Widgets.Add(shopButton);

        var testCombatButton = new Button { Content = new Label { Text = "Test Combat" } };
        testCombatButton.Click += (s, a) =>
        {
            _soundManager.PlaySound("button-click");
            var combatManager = new CombatManager(_partyManager);
            var enemies = new List<Character> { _resourceManager.Enemies[0], _resourceManager.Enemies[1] };
            combatManager.StartCombat(enemies);
            var combatScreen = new CombatScreen(combatManager);
            combatScreen.ShowModal(_desktop);
        };
        buttonPanel.Widgets.Add(testCombatButton);

        var pauseButton = new Button { Content = new Label { Text = "Pause" } };
        pauseButton.Click += (s, a) => { _game.TogglePause(); _soundManager.PlaySound("button-click"); };
        buttonPanel.Widgets.Add(pauseButton);

        _logButton = new Button { Content = new Label { Text = "Log" } };
        _logButton.Click += (s, a) => { _game.ToggleLogWindow(); _soundManager.PlaySound("button-click"); };
        buttonPanel.Widgets.Add(_logButton);

        _progressButton = new Button { Content = new Label { Text = "Progress" } };
        _progressButton.Click += (s, a) => { _game.ToggleTaskProgressWindow(); _soundManager.PlaySound("button-click"); };
        buttonPanel.Widgets.Add(_progressButton);

        SetRow(buttonPanel, 2);
        Widgets.Add(buttonPanel);
    }

    private void InitializePartyPanel()
    {
        var partyPanel = new PartyPanel(_partyManager)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        SetRow(partyPanel, 1);
        SetColumn(partyPanel, 2);
        Widgets.Add(partyPanel);
    }

    private void AddInitialCards()
    {
        foreach (var cardData in _resourceManager.Cards)
        {
            var cardWidget = new CardWidget(cardData, DropZone);
            _handPanel.Widgets.Add(cardWidget);
            CardWidgets.Add(cardWidget);
        }
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
        var flashScale = 1.0f + 0.1f * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 5);

        if (_game.NewLogAvailable)
        {
            _logButton.Scale = new Vector2(flashScale, flashScale);
        }
        else
        {
            _logButton.Scale = Vector2.One;
        }

        if (_game.NewTaskAvailable)
        {
            _progressButton.Scale = new Vector2(flashScale, flashScale);
        }
        else
        {
            _progressButton.Scale = Vector2.One;
        }
    }

    public void ResetCards()
    {
        _handPanel.Widgets.Clear();
        CardWidgets.Clear();
        AddInitialCards();
    }
}
