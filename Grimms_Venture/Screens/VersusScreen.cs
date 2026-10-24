using GrimmsVenture.Combat;
using GrimmsVenture.Core;
using GrimmsVenture.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Screens;

/// <summary>Partida versus local 1v1: intro, luta, fim de round, pausa. Câmera compartilhada ou tela dividida.</summary>
public sealed class VersusScreen : Screen
{
    enum Phase { Intro, Fight, RoundOver, Paused }

    readonly CameraRig _cams = new();
    readonly CombatSystem _combat = new();

    Fighter _p1 = null!;
    Fighter _p2 = null!;
    ViewMode _view;
    Phase _phase = Phase.Intro;
    Phase _resumePhase = Phase.Fight;
    float _phaseTime;
    string _result = "";
    bool _debug;

    public VersusScreen(GameContext ctx, ViewMode view) : base(ctx)
    {
        _view = view;
        _combat.ShakeRequested += _cams.AddShake;
        StartRound();
    }

    void StartRound()
    {
        _combat.Reset();
        _p1 = new Fighter(0, Roster.Grimm, new Vector3(-8f, 0f, 0f), Vector3.Right);
        _p2 = new Fighter(1, Roster.Wren, new Vector3(8f, 0f, 0f), Vector3.Left);
        _phase = Phase.Intro;
        _phaseTime = 0f;
        _result = "";
        _cams.Snap();
        _cams.Update(0f, _p1, _p2);
    }

    // ================================================================== update

    public override void Update(GameTime gt)
    {
        float dt = MathF.Min((float)gt.ElapsedGameTime.TotalSeconds, 1f / 20f);
        var input = Ctx.Input;

        if (input.ToggleViewPressed)
            _view = _view switch
            {
                ViewMode.Shared => ViewMode.SplitVertical,
                ViewMode.SplitVertical => ViewMode.SplitHorizontal,
                _ => ViewMode.Shared
            };
        if (input.DebugPressed) _debug = !_debug;

        switch (_phase)
        {
            case Phase.Paused:
                if (input.PauseRequested) _phase = _resumePhase;
                else if (input.MenuBack) { Ctx.Screens.Set(new TitleScreen(Ctx)); return; }
                return; // congela tudo (inclui câmera)

            case Phase.Intro:
                _phaseTime += dt;
                Simulate(dt, allowInput: false);
                if (_phaseTime >= 3.8f) { _phase = Phase.Fight; _phaseTime = 0f; }
                break;

            case Phase.Fight:
                if (input.PauseRequested) { _resumePhase = Phase.Fight; _phase = Phase.Paused; return; }
                Simulate(dt, allowInput: true);
                CheckKo();
                break;

            case Phase.RoundOver:
                _phaseTime += dt;
                Simulate(dt * (_phaseTime < 1.6f ? 0.35f : 1f), allowInput: false);
                if (_phaseTime > 1.8f)
                {
                    if (input.MenuConfirm) { StartRound(); return; }
                    if (input.MenuBack) { Ctx.Screens.Set(new TitleScreen(Ctx)); return; }
                }
                break;
        }

        _cams.Update(dt, _p1, _p2);
    }

    void Simulate(float dt, bool allowInput)
    {
        var in1 = allowInput ? Ctx.Input.Players[0] : default;
        var in2 = allowInput ? Ctx.Input.Players[1] : default;

        _combat.Particles.Update(dt);

        if (_combat.HitStop > 0f)   // hit-stop: congela a lógica por alguns frames no impacto
        {
            _combat.TickHitStop(dt);
            return;
        }

        _p1.Update(dt, in1, _p2, _combat);
        _p2.Update(dt, in2, _p1, _combat);
        _combat.Update(dt, _p1, _p2);
    }

    void CheckKo()
    {
        bool k1 = _p1.State == FighterState.KO;
        bool k2 = _p2.State == FighterState.KO;
        if (!k1 && !k2) return;

        _result = k1 && k2 ? "DRAW" : k1 ? _p2.Name + " WINS" : _p1.Name + " WINS";
        _phase = Phase.RoundOver;
        _phaseTime = 0f;
    }

    // ================================================================== draw

    public override void Draw(GameTime gt)
    {
        GraphicsDevice gd = Ctx.GraphicsDevice;
        int w = gd.PresentationParameters.BackBufferWidth;
        int h = gd.PresentationParameters.BackBufferHeight;
        var full = new Viewport(0, 0, w, h);

        gd.Viewport = full;
        gd.Clear(GameConstants.SkyColor);

        switch (_view)
        {
            case ViewMode.Shared:
                DrawView(full, _cams.Shared);
                break;
            case ViewMode.SplitVertical:
                DrawView(new Viewport(0, 0, w / 2, h), _cams.P1);
                DrawView(new Viewport(w / 2, 0, w - w / 2, h), _cams.P2);
                break;
            case ViewMode.SplitHorizontal:
                DrawView(new Viewport(0, 0, w, h / 2), _cams.P1);
                DrawView(new Viewport(0, h / 2, w, h - h / 2), _cams.P2);
                break;
        }

        gd.Viewport = full;
        DrawOverlay(new Rectangle(0, 0, w, h));
    }

    void DrawView(Viewport vp, Camera cam)
    {
        Ctx.GraphicsDevice.Viewport = vp;
        var r = Ctx.Renderer;

        r.BeginScene(cam, vp.AspectRatio);
        Ctx.Arena.DrawOpaque(r);
        FighterRenderer.DrawBody(r, _p1);
        FighterRenderer.DrawBody(r, _p2);

        r.BeginTransparent();
        FighterRenderer.DrawEffects(r, _p1, _debug);
        FighterRenderer.DrawEffects(r, _p2, _debug);
        FighterRenderer.DrawBlasts(r, _combat.Blasts);
        _combat.Particles.Draw(r);
        r.EndTransparent();
    }

    void DrawOverlay(Rectangle scr)
    {
        var sb = Ctx.SpriteBatch;
        var ui = Ctx.Ui;
        int px = Math.Max(2, scr.Height / 180);
        int big = px * 3;

        sb.Begin(samplerState: SamplerState.PointClamp);

        Ctx.Hud.DrawFight(sb, scr, _p1, _p2);

        // divisória da tela dividida
        if (_view == ViewMode.SplitVertical)
            ui.Rect(sb, new Rectangle(scr.Width / 2 - 2, 0, 4, scr.Height), Color.Black);
        else if (_view == ViewMode.SplitHorizontal)
            ui.Rect(sb, new Rectangle(0, scr.Height / 2 - 2, scr.Width, 4), Color.Black);

        int cy = scr.Height / 2 - big * 3;
        switch (_phase)
        {
            case Phase.Intro:
            {
                string t = _phaseTime < 3f ? (3 - (int)_phaseTime).ToString() : "FIGHT!";
                ui.TextCentered(sb, t, scr.Width / 2, cy, big * 2, new Color(255, 220, 100));
                ui.TextCentered(sb, $"P1: {Ctx.Input.DeviceNames[0]}    P2: {Ctx.Input.DeviceNames[1]}",
                    scr.Width / 2, scr.Height - px * 14, px, new Color(190, 190, 210));
                break;
            }
            case Phase.RoundOver:
                if (_phaseTime > 0.4f)
                    ui.TextCentered(sb, _result, scr.Width / 2, cy, big * 2, new Color(255, 220, 100));
                if (_phaseTime > 1.8f)
                    ui.TextCentered(sb, "ENTER OR A: REMATCH   BACKSPACE OR B: TITLE",
                        scr.Width / 2, cy + big * 10, px, Color.White);
                break;
            case Phase.Paused:
                ui.Rect(sb, scr, Color.Black * 0.55f);
                ui.TextCentered(sb, "PAUSED", scr.Width / 2, cy, big * 2, Color.White);
                ui.TextCentered(sb, "ESC OR START: RESUME   BACKSPACE OR B: QUIT TO TITLE",
                    scr.Width / 2, cy + big * 10, px, new Color(210, 210, 230));
                break;
        }

        // dica fixa
        if (_phase == Phase.Fight)
            ui.TextCentered(sb, "TAB/BACK: CAMERA   F1: DEBUG   F11: FULLSCREEN",
                scr.Width / 2, scr.Height - px * 8, px, new Color(150, 150, 170));

        if (_debug) DrawDebug(sb, px);

        sb.End();
    }

    void DrawDebug(SpriteBatch sb, int px)
    {
        var ui = Ctx.Ui;
        int x = px * 4;
        int y = px * 40;
        int line = px * 7;

        string[] lines =
        {
            $"FPS {Ctx.Fps:0}",
            $"VIEW {_view}",
            $"P1 {_p1.State} HP {_p1.Health:0} KI {_p1.Ki:0} Y {_p1.Position.Y:0.0}",
            $"P2 {_p2.State} HP {_p2.Health:0} KI {_p2.Ki:0} Y {_p2.Position.Y:0.0}",
            $"HITSTOP {_combat.HitStop:0.00}  BLASTS {_combat.Blasts.Count}"
        };

        for (int i = 0; i < lines.Length; i++)
            ui.Text(sb, lines[i], x, y + i * line, px, new Color(120, 255, 140));
    }
}
