using GrimmsVenture.Combat;
using GrimmsVenture.Controls;
using GrimmsVenture.Core;
using GrimmsVenture.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.Screens;

/// <summary>Seleção de personagem 1v1: cada jogador "entra" apertando um botão
/// (P1: qualquer botão | P2: START) e depois escolhe/confirma o personagem.</summary>
public sealed class CharacterSelectScreen : Screen
{
    sealed class Slot
    {
        public int Index;
        public int Choice;
        public bool Confirmed;
        public float PrevMoveX;
        public Fighter Preview = null!;
        public Fighter Dummy = null!;
        public readonly Camera Cam = new();
    }

    readonly ViewMode _view;
    readonly Slot[] _slots = new Slot[2];
    readonly CombatSystem _combat = new();
    float _time;
    float _startTimer = -1f;

    public CharacterSelectScreen(GameContext ctx, ViewMode view) : base(ctx)
    {
        _view = view;
        for (int i = 0; i < 2; i++)
        {
            _slots[i] = new Slot { Index = i };
            RebuildPreview(_slots[i]);
        }
    }

    void RebuildPreview(Slot s)
    {
        CharacterDefinition ch = Roster.All[s.Choice];
        Color c = s.Index == 0 ? ch.Color : ch.AltColor;
        // o "oponente" fica atrás da câmera, então o preview encara a tela
        s.Preview = new Fighter(s.Index, ch, Vector3.Zero, Vector3.Backward, c);
        s.Dummy = new Fighter(1 - s.Index, ch, new Vector3(0f, 0f, 30f), Vector3.Forward);
    }

    // ================================================================== update

    public override void Update(GameTime gt)
    {
        float dt = MathF.Min((float)gt.ElapsedGameTime.TotalSeconds, 1f / 20f);
        _time += dt;
        var input = Ctx.Input;
        PlayerInput none = default;

        bool consumed = false;
        for (int i = 0; i < 2; i++)
        {
            Slot s = _slots[i];

            if (!input.SlotJoined(i))
            {
                if (input.TryJoin(i)) consumed = true; // entrou agora; ignora o resto do frame
                s.PrevMoveX = 0f;
                s.Preview.Update(dt, none, s.Dummy, _combat); // anima o idle mesmo sem jogador
                continue;
            }

            PlayerInput p = input.Players[i];

            if (_startTimer < 0f)
            {
                if (s.Confirmed)
                {
                    if (p.DashPressed) { s.Confirmed = false; consumed = true; } // B / dash = cancelar
                }
                else
                {
                    int dir = 0;
                    if (p.Move.X > 0.6f && s.PrevMoveX <= 0.6f) dir = 1;
                    else if (p.Move.X < -0.6f && s.PrevMoveX >= -0.6f) dir = -1;

                    if (dir != 0)
                    {
                        s.Choice = (s.Choice + dir + Roster.All.Count) % Roster.All.Count;
                        RebuildPreview(s);
                    }
                    if (p.LightPressed) s.Confirmed = true;                       // A / golpe leve = confirmar
                }
            }

            s.PrevMoveX = p.Move.X;
            s.Preview.Update(dt, none, s.Dummy, _combat); // só para animar o idle
        }

        if (_startTimer < 0f)
        {
            if (!consumed && (input.MenuBack || input.EscapePressed))
            {
                Ctx.Screens.Set(new TitleScreen(Ctx));
                return;
            }
            if (_slots[0].Confirmed && _slots[1].Confirmed) _startTimer = 0f;
        }
        else
        {
            _startTimer += dt;
            if (_startTimer >= 0.8f)
            {
                Ctx.Screens.Set(new VersusScreen(Ctx, _view,
                    Roster.All[_slots[0].Choice], Roster.All[_slots[1].Choice]));
            }
        }
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

        DrawPreview(_slots[0], new Viewport(0, 0, w / 2, h));
        DrawPreview(_slots[1], new Viewport(w / 2, 0, w - w / 2, h));

        gd.Viewport = full;
        DrawOverlay(w, h);
    }

    void DrawPreview(Slot s, Viewport vp)
    {
        Ctx.GraphicsDevice.Viewport = vp;

        float a = 0.6f * MathF.Sin(_time * 0.6f + s.Index);
        s.Cam.Position = new Vector3(MathF.Sin(a) * 5.2f, 2.0f, MathF.Cos(a) * 5.2f);
        s.Cam.Target = new Vector3(0f, 1.15f, 0f);
        s.Cam.Update(0f);

        var r = Ctx.Renderer;
        r.BeginScene(s.Cam, vp.AspectRatio);
        Ctx.Arena.DrawOpaque(r);
        FighterRenderer.DrawBody(r, s.Preview);

        r.BeginTransparent();
        FighterRenderer.DrawEffects(r, s.Preview, false);
        r.EndTransparent();
    }

    void DrawOverlay(int w, int h)
    {
        var sb = Ctx.SpriteBatch;
        var ui = Ctx.Ui;
        int px = Math.Max(2, h / 180);
        int big = px * 3;
        int small = Math.Max(2, px - 1);
        int margin = w / 40;

        sb.Begin(samplerState: SamplerState.PointClamp);

        ui.Rect(sb, new Rectangle(w / 2 - 2, 0, 4, h), Color.Black);
        ui.TextCentered(sb, "CHARACTER SELECT", w / 2, px * 8, big, new Color(255, 200, 90));

        for (int i = 0; i < 2; i++)
        {
            Slot s = _slots[i];
            CharacterDefinition ch = Roster.All[s.Choice];
            Color col = i == 0 ? ch.Color : ch.AltColor;
            int x0 = i * (w / 2);
            int cx = x0 + w / 4;
            bool joined = Ctx.Input.SlotJoined(i);
            int panelY = (int)(h * 0.64f);

            ui.TextCentered(sb, $"PLAYER {i + 1}", cx, px * 8 + big * 8, px * 2, col);
            ui.Rect(sb, new Rectangle(x0 + margin, panelY, w / 2 - margin * 2, (int)(h * 0.27f)), Color.Black * 0.5f);

            if (!joined)
            {
                ui.TextCentered(sb,
                    i == 0 ? "PRESS ANY BUTTON TO JOIN" : "PRESS START TO JOIN",
                    cx, panelY + px * 3, px * 2, new Color(255, 220, 100));
                ui.TextCentered(sb,
                    i == 0 ? "KEYBOARD OR GAMEPAD" : "PAD: START  |  KEYBOARD: ANY P2 KEY",
                    cx, panelY + px * 3 + px * 14, small, new Color(170, 170, 190));
                continue;
            }

            string dev = Ctx.Input.DeviceNames[i];
            ui.TextCentered(sb, dev, cx, px * 8 + big * 8 + px * 14, small, new Color(170, 170, 190));

            string name = Roster.All.Count > 1 ? $"< {ch.Name} >" : ch.Name;
            ui.TextCentered(sb, name, cx, panelY + px * 3, big, col);
            ui.TextCentered(sb, ch.Tagline, cx, panelY + px * 3 + big * 7, px, new Color(200, 200, 220));
            ui.TextCentered(sb,
                $"HP {ch.Stats.MaxHealth:0}  SPD {ch.Stats.MoveSpeed:0.0}  PWR {ch.Stats.DamageMultiplier:0.00}",
                cx, panelY + px * 3 + big * 7 + px * 9, px, new Color(200, 200, 220));

            if (s.Confirmed)
                ui.TextCentered(sb, "READY!", cx, panelY + px * 3 + big * 7 + px * 20, px * 2, new Color(90, 230, 110));

            string hint = dev.StartsWith("GAMEPAD")
                ? "A: OK   B: CANCEL   STICK: CHANGE"
                : i == 0 ? "F: OK   LSHIFT: CANCEL   A/D: CHANGE"
                         : "J: OK   RSHIFT: CANCEL   ARROWS: CHANGE";
            ui.TextCentered(sb, hint, cx, h - px * 14, small, new Color(170, 170, 190));
        }

        if (_startTimer >= 0f)
            ui.TextCentered(sb, "FIGHT!", w / 2, h / 2 - big * 3, big * 2, new Color(255, 220, 100));
        else
            ui.TextCentered(sb, "BACKSPACE / ESC: BACK", w / 2, h - px * 7, small, new Color(140, 140, 160));

        sb.End();
    }
}