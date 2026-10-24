using GrimmsVenture.Core;
using GrimmsVenture.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Screens;

public sealed class TitleScreen : Screen
{
    static readonly string[] Items = { "VERSUS", "VERSUS SPLIT SCREEN", "QUIT" };

    readonly Camera _cam = new();
    int _selected;
    float _angle;

    public TitleScreen(GameContext ctx) : base(ctx) { }

    public override void Update(GameTime gt)
    {
        float dt = (float)gt.ElapsedGameTime.TotalSeconds;
        var input = Ctx.Input;

        if (input.MenuUp) _selected = (_selected + Items.Length - 1) % Items.Length;
        if (input.MenuDown) _selected = (_selected + 1) % Items.Length;

        if (input.EscapePressed) { Ctx.Game.Exit(); return; }

        if (input.MenuConfirm)
        {
            switch (_selected)
            {
                case 0: Ctx.Screens.Set(new VersusScreen(Ctx, ViewMode.Shared)); return;
                case 1: Ctx.Screens.Set(new VersusScreen(Ctx, ViewMode.SplitVertical)); return;
                default: Ctx.Game.Exit(); return;
            }
        }

        // câmera orbitando a arena ao fundo
        _angle += dt * 0.12f;
        _cam.Position = new Vector3(MathF.Cos(_angle) * 42f, 13f, MathF.Sin(_angle) * 42f);
        _cam.Target = new Vector3(0, 4f, 0);
        _cam.Update(dt);
    }

    public override void Draw(GameTime gt)
    {
        GraphicsDevice gd = Ctx.GraphicsDevice;
        int w = gd.PresentationParameters.BackBufferWidth;
        int h = gd.PresentationParameters.BackBufferHeight;
        var full = new Viewport(0, 0, w, h);

        gd.Viewport = full;
        gd.Clear(GameConstants.SkyColor);

        Ctx.Renderer.BeginScene(_cam, full.AspectRatio);
        Ctx.Arena.DrawOpaque(Ctx.Renderer);

        var sb = Ctx.SpriteBatch;
        var ui = Ctx.Ui;
        int titlePx = Math.Max(4, h / 60);
        int menuPx = Math.Max(2, h / 150);

        sb.Begin(samplerState: SamplerState.PointClamp);

        ui.TextCentered(sb, GameConstants.Title, w / 2, (int)(h * 0.16f), titlePx, new Color(255, 200, 90));
        ui.TextCentered(sb, "PROTOTYPE", w / 2, (int)(h * 0.16f) + titlePx * 7, menuPx, new Color(200, 200, 220));

        int y = (int)(h * 0.52f);
        ui.Rect(sb, new Rectangle(w / 2 - (int)(w * 0.2f), y - menuPx * 3, (int)(w * 0.4f), Items.Length * menuPx * 9 + menuPx * 4),
            Color.Black * 0.45f);

        for (int i = 0; i < Items.Length; i++)
        {
            bool sel = i == _selected;
            string text = sel ? "> " + Items[i] : Items[i];
            ui.TextCentered(sb, text, w / 2, y + i * menuPx * 9, menuPx, sel ? Color.White : new Color(150, 150, 170));
        }

        ui.TextCentered(sb, "W/S OR DPAD: SELECT   ENTER OR A: CONFIRM   ESC: QUIT",
            w / 2, h - menuPx * 8, Math.Max(2, menuPx - 1), new Color(170, 170, 190));

        sb.End();
    }
}
