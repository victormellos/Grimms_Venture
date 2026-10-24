using GrimmsVenture.Combat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrimmsVenture.UI;

public sealed class Hud
{
    readonly Ui _ui;
    public Hud(Ui ui) => _ui = ui;

    public void DrawFight(SpriteBatch sb, Rectangle scr, Fighter p1, Fighter p2)
    {
        float s = scr.Height / 720f;
        int margin = (int)(28 * s);
        int barW = (int)(scr.Width * 0.38f);
        int barH = Math.Max(8, (int)(24 * s));
        int y = (int)(44 * s);

        DrawPlayer(sb, p1, new Rectangle(margin, y, barW, barH), true, s);
        DrawPlayer(sb, p2, new Rectangle(scr.Width - margin - barW, y, barW, barH), false, s);
    }

    void DrawPlayer(SpriteBatch sb, Fighter f, Rectangle r, bool left, float s)
    {
        int px = Math.Max(2, (int)MathF.Round(3 * s));
        int border = Math.Max(2, (int)(3 * s));

        // vida
        Rectangle frame = r;
        frame.Inflate(border, border);
        _ui.Rect(sb, frame, Color.Black);
        _ui.Rect(sb, r, new Color(50, 50, 60));

        float pct = MathHelper.Clamp(f.Health / f.Stats.MaxHealth, 0f, 1f);
        int w = (int)(r.Width * pct);
        Color hc = pct > 0.5f ? new Color(80, 220, 90) : pct > 0.25f ? new Color(240, 200, 60) : new Color(230, 70, 60);
        _ui.Rect(sb, left ? new Rectangle(r.X, r.Y, w, r.Height) : new Rectangle(r.Right - w, r.Y, w, r.Height), hc);

        // ki
        int kiH = Math.Max(5, (int)(10 * s));
        int kiW = (int)(r.Width * 0.6f);
        int kiY = r.Bottom + (int)(8 * s);
        Rectangle kr = left ? new Rectangle(r.X, kiY, kiW, kiH) : new Rectangle(r.Right - kiW, kiY, kiW, kiH);
        Rectangle kf = kr;
        kf.Inflate(2, 2);
        _ui.Rect(sb, kf, Color.Black);
        _ui.Rect(sb, kr, new Color(30, 40, 60));

        float kp = MathHelper.Clamp(f.Ki / f.Stats.MaxKi, 0f, 1f);
        int kw = (int)(kiW * kp);
        Color kc = kp >= 0.999f ? new Color(255, 220, 90) : new Color(70, 190, 255);
        _ui.Rect(sb, left ? new Rectangle(kr.X, kr.Y, kw, kr.Height) : new Rectangle(kr.Right - kw, kr.Y, kw, kr.Height), kc);

        // nome
        int nameY = r.Y - px * 5 - (int)(10 * s);
        int nameX = left ? r.X : r.Right - _ui.TextWidth(f.Name, px);
        _ui.Text(sb, f.Name, nameX, nameY, px, f.Color);

        // combo
        if (f.ComboCount >= 2)
        {
            string t = $"{f.ComboCount} HITS";
            int cpx = px + 1;
            int cx = left ? r.X : r.Right - _ui.TextWidth(t, cpx);
            _ui.Text(sb, t, cx, kr.Bottom + (int)(14 * s), cpx, new Color(255, 160, 40));
        }
    }
}
