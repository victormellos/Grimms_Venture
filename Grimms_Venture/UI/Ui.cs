using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.UI;

/// <summary>
/// Desenho 2D sem assets: retângulos + fonte bitmap 3x5 embutida.
/// (Troque por SpriteFont/Content Pipeline quando quiser fontes bonitas e acentos.)
/// </summary>
public sealed class Ui : IDisposable
{
    const int GlyphW = 3, GlyphH = 5, Spacing = 1;

    static readonly Dictionary<char, string> Glyphs = new()
    {
        ['A'] = "111101111101101", ['B'] = "110101110101110", ['C'] = "011100100100011",
        ['D'] = "110101101101110", ['E'] = "111100110100111", ['F'] = "111100110100100",
        ['G'] = "011100101101011", ['H'] = "101101111101101", ['I'] = "111010010010111",
        ['J'] = "001001001101010", ['K'] = "101101110101101", ['L'] = "100100100100111",
        ['M'] = "101111111101101", ['N'] = "111101101101101", ['O'] = "010101101101010",
        ['P'] = "111101111100100", ['Q'] = "010101101111011", ['R'] = "110101110101101",
        ['S'] = "011100010001110", ['T'] = "111010010010010", ['U'] = "101101101101111",
        ['V'] = "101101101101010", ['W'] = "101101111111101", ['X'] = "101101010101101",
        ['Y'] = "101101010010010", ['Z'] = "111001010100111",
        ['0'] = "111101101101111", ['1'] = "010110010010111", ['2'] = "111001111100111",
        ['3'] = "111001111001111", ['4'] = "101101111001001", ['5'] = "111100111001111",
        ['6'] = "111100111101111", ['7'] = "111001001001001", ['8'] = "111101111101111",
        ['9'] = "111101111001111",
        ['!'] = "010010010000010", ['.'] = "000000000000010", ['-'] = "000000111000000",
        [':'] = "000010000010000", ['\''] = "010010000000000", ['/'] = "001001010100100",
        ['>'] = "100010001010100", ['+'] = "000010111010000", ['?'] = "111001010000010",
        [','] = "000000000010100", [' '] = "000000000000000"
    };

    readonly Texture2D _px;

    public Ui(GraphicsDevice gd)
    {
        _px = new Texture2D(gd, 1, 1);
        _px.SetData(new[] { Color.White });
    }

    public void Rect(SpriteBatch sb, Rectangle r, Color c) => sb.Draw(_px, r, c);

    public int TextWidth(string text, int px)
        => text.Length == 0 ? 0 : (text.Length * (GlyphW + Spacing) - Spacing) * px;

    public void Text(SpriteBatch sb, string text, int x, int y, int px, Color color, bool shadow = true)
    {
        if (shadow) DrawGlyphs(sb, text, x + px, y + px, px, Color.Black * 0.7f);
        DrawGlyphs(sb, text, x, y, px, color);
    }

    public void TextCentered(SpriteBatch sb, string text, int centerX, int y, int px, Color color, bool shadow = true)
        => Text(sb, text, centerX - TextWidth(text, px) / 2, y, px, color, shadow);

    void DrawGlyphs(SpriteBatch sb, string text, int x, int y, int px, Color color)
    {
        int cx = x;
        foreach (char raw in text)
        {
            char ch = char.ToUpperInvariant(raw);
            if (Glyphs.TryGetValue(ch, out string? g))
            {
                for (int row = 0; row < GlyphH; row++)
                    for (int col = 0; col < GlyphW; col++)
                        if (g[row * GlyphW + col] == '1')
                            sb.Draw(_px, new Rectangle(cx + col * px, y + row * px, px, px), color);
            }
            cx += (GlyphW + Spacing) * px;
        }
    }

    public void Dispose() => _px.Dispose();
}
