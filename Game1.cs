using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Mythril.GameLogic;
using Mythril.UI;
using AssetManagementBase;

namespace Mythril;

public class Game1 : Game
{
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _resourceManager = new ResourceManager();
        _taskManager = new TaskManager(_resourceManager);
        _gameManager = new GameManager(_resourceManager); // Pass resourceManager to GameManager
        _assetManager =  AssetManager.CreateFileAssetManager("Content");
        _gameManager.OnGameOver += HandleGameOver; // Subscribe to game over event
    }

    private readonly GraphicsDeviceManager _graphics;
    private Desktop? _desktop;
    private MainLayout? _mainLayout; // Reference to the main UI layout
    private CardWidget? _draggedCard; // To track the currently dragged card
    private static TextBox? _logTextBox; // Static for easy access
    private readonly ResourceManager _resourceManager;
    private readonly TaskManager _taskManager;
    private readonly GameManager _gameManager; // Add GameManager field
    private readonly AssetManager _assetManager;

    protected override void LoadContent()
    {
        MyraEnvironment.Game = this;
        _assetManager.Open("DefaultSkin.xml");

        _mainLayout = new MainLayout(this, _taskManager, _desktop, _resourceManager);
        _desktop = new Desktop
        {
            Root = _mainLayout
        };

        // Subscribe to OnDragEnd for all initial CardWidgets
        foreach (var cardWidget in _mainLayout.CardWidgets)
        {
            cardWidget.OnDragEnd += HandleCardDragEnd;
        }

        // Initialize log text box
        _logTextBox = new TextBox
        {
            Width = 400,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Multiline = true // Equivalent to Wrap for TextBox
        };
        _desktop.Widgets.Add(_logTextBox); // Add to desktop directly for now
    }

    public static void Log(string message)
    {
        Console.WriteLine(message);
        if (_logTextBox != null)
        {
            _logTextBox.Text += message + "\n";
        }
    }

    private void HandleCardDragEnd(CardWidget cardWidget) => _draggedCard = cardWidget;

    protected override void Update(GameTime gameTime)
    {
        if (!_isPaused)
        {
            _taskManager.Update(gameTime);
            _mainLayout?.Update(gameTime);
            _mainLayout?.UpdateResources(_resourceManager);
        }

        // Handle card drop
        if (_draggedCard != null && Mouse.GetState().LeftButton == ButtonState.Released)
        {
            _mainLayout?.HandleCardDrop(_draggedCard);
            _draggedCard = null;
        }

        // Toggle fullscreen on F11 press
        if (Keyboard.GetState().IsKeyDown(Keys.F11) && _lastKeyboardState.IsKeyUp(Keys.F11))
        {
            ToggleFullscreen();
        }
        _lastKeyboardState = Keyboard.GetState();

        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _desktop?.Render();

        base.Draw(gameTime);
    }

    private KeyboardState _lastKeyboardState;
    private bool _isPaused = false;

    public void ToggleFullscreen() => _graphics.ToggleFullScreen();

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        _taskManager.SetPaused(_isPaused);
        Log($"Game paused: {_isPaused}");
    }

    private void HandleGameOver()
    {
        Log("Game Over!");
        _isPaused = true; // Pause the game on game over
        var gameOverDialog = new GameOverDialog();
        gameOverDialog.OnRestartGame += RestartGame;
        gameOverDialog.ShowModal(_desktop);
    }

    private void RestartGame()
    {
        // Reset game state
        _resourceManager.Reset();
        _taskManager.Reset(); // You'll need to implement a Reset method in TaskManager
        _mainLayout?.ResetCards(); // Reset cards in MainLayout
        _isPaused = false;
        Log("Game Restarted!");
    }
}
