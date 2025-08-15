using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Mythril.GameLogic;
using Mythril.UI;
using AssetManagementBase;
using Mythril.API;
using Mythril.API.Transport;
using System.Collections.Generic;
using Mythril.GameLogic.AI;
using System.IO;

namespace Mythril;

public class Game1 : Game
{
    public static Game1? Instance { get; private set; }

    public bool NewLogAvailable { get; private set; }
    public bool NewTaskAvailable { get; private set; }

    private readonly GraphicsDeviceManager _graphics;
    private Desktop? _desktop;
    private MainLayout? _mainLayout;
    private SpriteBatch? _spriteBatch;
    private RenderTarget2D? _finalRenderTarget;
    private CardWidget? _draggedCard;

    private static LogWindow? _logWindow;
    public static CombatScreen? _combatScreen;
    private static readonly List<string> _logMessages = new();
    private TaskProgressWindow? _taskProgressWindow;

    private readonly ResourceManager _resourceManager;
    private readonly TaskManager _taskManager;
    private readonly GameManager _gameManager;
    private readonly AssetManager _assetManager;
    private readonly SoundManager _soundManager;
    private CommandListener? _commandListener;
    private readonly Stack<ICommandExecutor> _commandExecutorStack = new();
    private CommandExecutor? _commandExecutor;
    private Action<string>? _screenshotCallback;

    public SoundManager SoundManager => _soundManager;

    public Game1(ICommandTransport? transport = null)
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

        if (transport is not null)
        {
            _commandListener = new CommandListener(transport);
        }

        _gameManager.OnGameOver += HandleGameOver;
        _taskManager.OnTaskStarted += OnTaskStarted;
        _taskManager.OnTaskCompleted += OnTaskCompleted;
    }

    protected override void LoadContent()
    {
        MyraEnvironment.Game = this;
        _assetManager.Open("DefaultSkin.xml");

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _finalRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

        _desktop = new Desktop();
        _mainLayout = new MainLayout(this, _taskManager, _desktop, _resourceManager, _soundManager);
        _desktop.Root = _mainLayout;

        // AI Command Integration
        if (_commandListener is not null)
        {
            _commandExecutor = new CommandExecutor(this, _desktop);
            _commandExecutorStack.Push(_commandExecutor);
            _commandListener.StartListening();
        }

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
        _combatScreen?.AddLogMessage(message);
        if (Instance != null) Instance.NewLogAvailable = true;
    }

    private void HandleCardDragEnd(CardWidget cardWidget) => _draggedCard = cardWidget;

    private KeyboardState _lastKeyboardState;
    private bool _isPaused = false;
    private bool _isExecutingCommand = false;

    protected override void Update(GameTime gameTime)
    {
        if (!_isPaused)
        {
            _taskManager.Update(gameTime);
            _mainLayout?.Update(gameTime);
            _taskProgressWindow?.UpdateTasks();
            _mainLayout?.UpdateResources(_resourceManager);

            // Process AI commands
            if (_commandListener != null && _commandExecutorStack.Count > 0 && !_isExecutingCommand)
            {
                if (_commandListener.TryDequeueCommand(out var command))
                {
                    if (command != null)
                    {
                        _isExecutingCommand = true;
                        Task.Run(async () =>
                        {
                            if (_commandExecutorStack.TryPeek(out var executor))
                            {
                                await executor.ExecuteCommand(command);
                            }
                            _isExecutingCommand = false;
                        });
                    }
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
        // Draw scene to the render target
        if (_finalRenderTarget is not null)
        {
            GraphicsDevice.SetRenderTarget(_finalRenderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _desktop?.Render();
        }

        // Handle screenshot request
        if (_screenshotCallback is not null && _finalRenderTarget is not null)
        {
            using var stream = new MemoryStream();
            _finalRenderTarget.SaveAsPng(stream, _finalRenderTarget.Width, _finalRenderTarget.Height);
            var base64String = "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
            _screenshotCallback(base64String);
            _screenshotCallback = null;
        }

        // Draw the render target to the back buffer
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        if (_spriteBatch is not null && _finalRenderTarget is not null)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_finalRenderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

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

    public void RequestScreenshot(Action<string> callback)
    {
        _screenshotCallback = callback;
    }

    public void PushCommandExecutor(ICommandExecutor executor)
    {
        _commandExecutorStack.Push(executor);
    }

    public void PopCommandExecutor()
    {
        if (_commandExecutorStack.Count > 1)
            _commandExecutorStack.Pop();
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
