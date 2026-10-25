using GrimmsVenture.Combat;
using GrimmsVenture.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.UI;

/// <summary>
/// Dica de controles pequena no canto da tela (P1 embaixo-esquerda, P2 embaixo-direita).
/// Mostra so as acoes que o personagem tem, com os botoes do dispositivo em uso:
/// teclado (teclas), gamepad Xbox (A/B/X/Y) ou PlayStation (cruz/bola/quadrado/triangulo).
/// </summary>
public sealed class ControlsHud
{
    readonly Ui _ui;
    public ControlsHud(Ui ui) => _ui = ui;

    // badge 7x7 com cantos cortados
    static readonly string[] Badge =
    {
        "0111110", "1111111", "1111111", "1111111", "1111111", "1111111", "0111110"
    };

    static readonly string[] Cross    = { "10001", "01010", "00100", "01010", "10001" };
    static readonly string[] Circle   = { "01110", "10001", "10001", "10001", "01110" };
    static readonly string[] Square   = { "11111", "10001", "10001", "10001", "11111" };
    static readonly string[] Triangle = { "00100", "01010", "01010", "10001", "11111" };

    public void Draw(SpriteBatch sb, Rectangle scr, CharacterDefinition c1, CharacterDefinition c2, InputManager input)
    {
        DrawPanel(sb, scr, c1, input.GetDevice(0), 0);
        DrawPanel(sb, scr, c2, input.GetDevice(1), 1);
    }

    void DrawPanel(SpriteBatch sb, Rectangle scr, CharacterDefinition ch, DeviceInfo dev, int slot)
    {
        if (dev.Kind == DeviceKind.None) return;

        float s = scr.Height / 720f;
        int px = Math.Max(2, (int)MathF.Round(2f * s));
        int margin = Math.Max(4, (int)(14 * s));
        int pad = 2 * px;
        int rowH = 8 * px;

        var hints = ControlHints.Build(ch, dev);
        if (hints.Count == 0) return;

        string device = dev.Kind == DeviceKind.Keyboard ? "KEYBOARD"
                      : dev.PadStyle == PadStyle.PlayStation ? "PLAYSTATION" : "GAMEPAD";
        string title = $"P{slot + 1} {device}";

        int bindW = 0, labelW = 0;
        foreach (ControlHint h in hints)
        {
            bindW = Math.Max(bindW, BindingWidth(h, dev.PadStyle, px));
            labelW = Math.Max(labelW, _ui.TextWidth(h.Label, px));
        }

        int w = Math.Max(pad * 2 + bindW + 2 * px + labelW, _ui.TextWidth(title, px) + pad * 2);
        int titleH = 5 * px + 2 * px;
        int hgt = pad * 2 + titleH + rowH * hints.Count;

        int x = slot == 0 ? scr.X + margin : scr.Right - margin - w;
        int y = scr.Bottom - margin - hgt;

        _ui.Rect(sb, new Rectangle(x, y, w, hgt), Color.Black * 0.45f);
        Color tint = slot == 0 ? ch.Color : ch.AltColor;
        _ui.Text(sb, title, x + pad, y + pad, px, tint);

        int ry = y + pad + titleH;
        foreach (ControlHint h in hints)
        {
            DrawBinding(sb, h, dev.PadStyle, x + pad, ry, px);
            _ui.Text(sb, h.Label, x + pad + bindW + 2 * px, ry + px, px, new Color(225, 225, 235));
            ry += rowH;
        }
    }

    // ------------------------------------------------------------ bindings

    int CapWidth(string text, int px) => _ui.TextWidth(text, px) + 4 * px;

    static bool IsFace(PadButton b) => b is PadButton.A or PadButton.B or PadButton.X or PadButton.Y;

    int BindingWidth(ControlHint h, PadStyle style, int px)
    {
        if (h.Pad is { } b)
            return IsFace(b) ? 7 * px : CapWidth(ControlHints.PadText(b, style), px);
        return CapWidth(h.KeyText ?? "", px);
    }

    void DrawBinding(SpriteBatch sb, ControlHint h, PadStyle style, int x, int y, int px)
    {
        if (h.Pad is { } b && IsFace(b)) DrawFace(sb, b, style, x, y, px);
        else DrawCap(sb, h.Pad is { } pb ? ControlHints.PadText(pb, style) : (h.KeyText ?? ""), x, y, px);
    }

    // tecla / ombro / gatilho: caixinha com texto
    void DrawCap(SpriteBatch sb, string text, int x, int y, int px)
    {
        int w = CapWidth(text, px);
        _ui.Rect(sb, new Rectangle(x, y, w, 7 * px), new Color(150, 150, 170));
        _ui.Rect(sb, new Rectangle(x + px, y + px, w - 2 * px, 5 * px), new Color(35, 35, 48));
        _ui.Text(sb, text, x + 2 * px, y + px, px, Color.White, shadow: false);
    }

    // A/B/X/Y: Xbox = bolinha colorida com letra; PlayStation = simbolo colorido
    void DrawFace(SpriteBatch sb, PadButton b, PadStyle style, int x, int y, int px)
    {
        bool ps = style == PadStyle.PlayStation;

        Color badge = ps ? new Color(35, 35, 48) : b switch
        {
            PadButton.A => new Color(80, 200, 90),
            PadButton.B => new Color(230, 80, 70),
            PadButton.X => new Color(70, 140, 240),
            _ => new Color(240, 200, 60)
        };
        DrawBitmap(sb, Badge, x, y, px, badge);

        if (ps)
        {
            string[] sym = b switch
            {
                PadButton.A => Cross,
                PadButton.B => Circle,
                PadButton.X => Square,
                _ => Triangle
            };
            Color c = b switch
            {
                PadButton.A => new Color(120, 170, 255),
                PadButton.B => new Color(255, 100, 110),
                PadButton.X => new Color(240, 140, 200),
                _ => new Color(90, 220, 170)
            };
            DrawBitmap(sb, sym, x + px, y + px, px, c);
        }
        else
        {
            _ui.Text(sb, b.ToString(), x + 2 * px, y + px, px, new Color(20, 20, 28), shadow: false);
        }
    }

    void DrawBitmap(SpriteBatch sb, string[] rows, int x, int y, int px, Color c)
    {
        for (int r = 0; r < rows.Length; r++)
            for (int col = 0; col < rows[r].Length; col++)
                if (rows[r][col] == '1')
                    _ui.Rect(sb, new Rectangle(x + col * px, y + r * px, px, px), c);
    }
}