using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Mythril.GameLogic;
using Mythril.UI;
using AssetManagementBase;
using Mythril.GameLogic.AI;
using Mythril.Controller.Transport;

namespace Mythril;

public class Game1 : Game
{
    public static Game1? Instance { get; private set; }

    public bool NewLogAvailable { get; private set; }
    public bool NewTaskAvailable { get; private set; }

    private readonly GraphicsDeviceManager _graphics;
    private Desktop? _desktop;
    private MainLayout? _mainLayout;
    private CardWidget? _draggedCard;

    private static LogWindow? _logWindow;
    private static readonly List<string> _logMessages = new();
    private TaskProgressWindow? _taskProgressWindow;

    private readonly ResourceManager _resourceManager;
    private readonly TaskManager _taskManager;
    private readonly GameManager _gameManager;
    private readonly AssetManager _assetManager;
    private readonly SoundManager _soundManager;
    private readonly ScreenshotUtility _screenshotUtility;
    private CommandListener? _commandListener;
    private CommandExecutor? _commandExecutor;

    public SoundManager SoundManager => _soundManager;

    public Game1()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this)
        {
            HardwareModeSwitch = false
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _resourceManager = new ResourceManager();
        _taskManager = new TaskManager(_resourceManager);
        _gameManager = new GameManager(_resourceManager);
        _assetManager =  AssetManager.CreateFileAssetManager("Content");
        _soundManager = new SoundManager(Content);
        _screenshotUtility = new ScreenshotUtility(GraphicsDevice);

        _gameManager.OnGameOver += HandleGameOver;
        _taskManager.OnTaskStarted += OnTaskStarted;
        _taskManager.OnTaskCompleted += OnTaskCompleted;
    }

    protected override void LoadContent()
    {
        MyraEnvironment.Game = this;
        _assetManager.Open("DefaultSkin.xml");

        _desktop = new Desktop();
        _mainLayout = new MainLayout(this, _taskManager, _desktop, _resourceManager, _soundManager);
        _desktop.Root = _mainLayout;

        // AI Command Integration
        var transport = new StdIoTransport(); // Default to StdIO for now
        _commandListener = new CommandListener(transport);
        _commandExecutor = new CommandExecutor(this, _desktop, _screenshotUtility);
        _commandListener.StartListening();

        _soundManager.LoadMusic("main-theme", "Music/main-theme");
        _soundManager.PlayMusic("main-theme");

        foreach (var cardWidget in _mainLayout.CardWidgets)
        {
            cardWidget.OnDragEnd += HandleCardDragEnd;
        }
    }

    public static void Log(string message)
    {
        Console.WriteLine(message);
        _logMessages.Add(message);
        _logWindow?.AddLog(message);
        if (Instance != null) Instance.NewLogAvailable = true;
    }

    private void HandleCardDragEnd(CardWidget cardWidget) => _draggedCard = cardWidget;

    private KeyboardState _lastKeyboardState;
    private bool _isPaused = false;

    protected override void Update(GameTime gameTime)
    {
        if (!_isPaused)
        {
            _taskManager.Update(gameTime);
            _mainLayout?.Update(gameTime);
            _taskProgressWindow?.UpdateTasks();
            _mainLayout?.UpdateResources(_resourceManager);

            // Process AI commands
            if (_commandListener != null && _commandExecutor != null)
            {
                while (_commandListener.TryDequeueCommand(out var command))
                {
                    _ = _commandExecutor.ExecuteCommand(command); // Don't await to avoid blocking game loop
                }
            }
        }

        if (_draggedCard != null && Mouse.GetState().LeftButton == ButtonState.Released)
        {
            // Remove card from desktop
            _desktop?.Widgets.Remove(_draggedCard);

            if (_mainLayout != null && _mainLayout.DropZone.Bounds.Contains(Mouse.GetState().Position))
            {
                _mainLayout.DropZone.HandleDrop(_draggedCard);
            }
            else
            {
                // Return to original parent
                _draggedCard.OriginalParent?.Widgets.Insert(_draggedCard.OriginalIndex, _draggedCard);
            }
            _draggedCard = null;
        }

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

    public void ToggleFullscreen()
    {
        if (Window.IsBorderless)
        {
            // Go windowed
            Window.IsBorderless = false;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }
        else
        {
            // Go fullscreen borderless
            Window.IsBorderless = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        _graphics.ApplyChanges();
    }

    public void ToggleLogWindow()
    {
        if (_desktop is null) return;

        if (_logWindow is not null)
        {
            _logWindow.Close();
            _logWindow = null;
            return;
        }

        NewLogAvailable = false;
        _logWindow = new LogWindow();
        foreach (var message in _logMessages)
        {
            _logWindow.AddLog(message);
        }
        _logWindow.Show(_desktop);
    }

    public void ToggleTaskProgressWindow()
    {
        if (_desktop is null) return;

        if (_taskProgressWindow is not null)
        {
            _taskProgressWindow.Close();
            _taskProgressWindow = null;
            return;
        }

        NewTaskAvailable = false;
        _taskProgressWindow = new TaskProgressWindow();
        _taskProgressWindow.Show(_desktop);

        foreach (var task in _taskManager.GetActiveTasks())
        {
            _taskProgressWindow.AddTask(task);
        }
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        _taskManager.SetPaused(_isPaused);
        Log($"Game paused: {_isPaused}");
    }

    private void HandleGameOver()
    {
        Log("Game Over!");
        _isPaused = true;
        var gameOverDialog = new GameOverDialog();
        gameOverDialog.OnRestartGame += RestartGame;
        gameOverDialog.ShowModal(_desktop);
    }

    private void RestartGame()
    {
        _resourceManager.Reset();
        _taskManager.Reset();
        _mainLayout?.ResetCards();
        _isPaused = false;
        Log("Game Restarted!");
    }

    private void OnTaskStarted(TaskProgress task)
    {
        NewTaskAvailable = true;
        _taskProgressWindow?.AddTask(task);
    }

    private void OnTaskCompleted(TaskProgress task) => _taskProgressWindow?.RemoveTask(task);
}
