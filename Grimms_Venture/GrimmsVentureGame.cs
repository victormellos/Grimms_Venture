using GrimmsVenture.Controls;
using GrimmsVenture.Core;
using GrimmsVenture.Rendering;
using GrimmsVenture.Screens;
using GrimmsVenture.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture;

public sealed class GrimmsVentureGame : Game
{
    readonly GraphicsDeviceManager _graphics;
    GameContext _ctx = null!;
    bool _resizing;
    int _frames;
    float _fpsTimer;

    public GrimmsVentureGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            GraphicsProfile = GraphicsProfile.HiDef,
            SynchronizeWithVerticalRetrace = true,
            HardwareModeSwitch = false // fullscreen "borderless"
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;                       // lógica a 60 Hz fixos (bom para luta e futuro netcode)
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        Window.Title = "Grimm's Venture";
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        Window.ClientSizeChanged += OnClientSizeChanged;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        var ui = new Ui(GraphicsDevice);
        _ctx = new GameContext
        {
            Game = this,
            GraphicsDevice = GraphicsDevice,
            SpriteBatch = new SpriteBatch(GraphicsDevice),
            Input = new InputManager(),
            Renderer = new Renderer3D(GraphicsDevice),
            Arena = new ArenaRenderer(GraphicsDevice),
            Ui = ui,
            Hud = new Hud(ui),
            Screens = new ScreenManager()
        };
        _ctx.Screens.Set(new TitleScreen(_ctx));
    }

    protected override void Update(GameTime gameTime)
    {
        _ctx.Input.Update();

        if (_ctx.Input.FullscreenPressed)
            _graphics.ToggleFullScreen();

        _ctx.Screens.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _ctx.Screens.Draw(gameTime);

        _frames++;
        _fpsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_fpsTimer >= 0.5f)
        {
            _ctx.Fps = _frames / _fpsTimer;
            _frames = 0;
            _fpsTimer = 0f;
        }

        base.Draw(gameTime);
    }

    void OnClientSizeChanged(object? sender, EventArgs e)
    {
        if (_resizing) return;
        var b = Window.ClientBounds;
        if (b.Width <= 0 || b.Height <= 0) return;

        _resizing = true;
        _graphics.PreferredBackBufferWidth = b.Width;
        _graphics.PreferredBackBufferHeight = b.Height;
        _graphics.ApplyChanges();
        _resizing = false;
    }
}
