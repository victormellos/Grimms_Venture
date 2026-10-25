using GrimmsVenture.Combat;
using Microsoft.Xna.Framework.Input;

namespace GrimmsVenture.Controls;

public enum DeviceKind { None, Keyboard, Gamepad }

/// <summary>Layout dos rotulos do gamepad. Os botoes fisicos sao os mesmos (A = baixo = Cruz).</summary>
public enum PadStyle { Xbox, PlayStation }

public enum PadButton { A, B, X, Y, LB, RB, LT, RT, LStick, Triggers }

/// <summary>Qual dispositivo um slot esta usando (para mostrar os controles certos).</summary>
public readonly record struct DeviceInfo(DeviceKind Kind, PadStyle PadStyle, KeyboardMap? Keys);

/// <summary>Uma linha da dica de controles: rotulo + (botao do pad OU texto de tecla).</summary>
public readonly record struct ControlHint(string Label, PadButton? Pad, string? KeyText);

public static class ControlHints
{
    /// <summary>
    /// Lista de acoes que ESTE personagem tem, com o botao/tecla de cada uma.
    /// Segue as capacidades do CharacterDefinition: voa ou pula, tem ki blast?, tem ki (carregar)?
    /// </summary>
    public static List<ControlHint> Build(CharacterDefinition c, DeviceInfo d)
    {
        var list = new List<ControlHint>();
        if (d.Kind == DeviceKind.None) return list;

        bool pad = d.Kind == DeviceKind.Gamepad;
        KeyboardMap k = d.Keys ?? KeyboardMap.Player1();

        ControlHint H(string label, PadButton padBtn, string keyText)
            => pad ? new ControlHint(label, padBtn, null) : new ControlHint(label, null, keyText);

        list.Add(H("MOVE", PadButton.LStick, MoveKeys(k)));

        if (c.Stats.CanFly)
            list.Add(H("FLY UP/DOWN", PadButton.Triggers, KeyName(k.Up) + "/" + KeyName(k.Down)));
        else
            list.Add(H(c.Stats.MaxAirJumps > 0 ? "JUMP/DOUBLE JUMP" : "JUMP", PadButton.RT, KeyName(k.Up)));

        list.Add(H(c.LightLabel, PadButton.A, KeyName(k.Light)));
        if (c.Attacks.Heavy != null)   list.Add(H(c.HeavyLabel, PadButton.X, KeyName(k.Heavy)));
        if (c.Attacks.KiBlast != null) list.Add(H("KI BLAST", PadButton.Y, KeyName(k.KiBlast)));
        list.Add(H("DASH", PadButton.B, KeyName(k.Dash)));
        list.Add(H("GUARD", PadButton.LB, KeyName(k.Guard)));
        if (c.Stats.HasKi) list.Add(H("CHARGE KI", PadButton.RB, KeyName(k.Charge)));

        return list;
    }

    /// <summary>Texto do botao para botoes que nao sao A/B/X/Y (ombros, gatilhos, analogico).</summary>
    public static string PadText(PadButton b, PadStyle style)
    {
        bool ps = style == PadStyle.PlayStation;
        return b switch
        {
            PadButton.LB => ps ? "L1" : "LB",
            PadButton.RB => ps ? "R1" : "RB",
            PadButton.LT => ps ? "L2" : "LT",
            PadButton.RT => ps ? "R2" : "RT",
            PadButton.Triggers => ps ? "R2/L2" : "RT/LT",
            PadButton.LStick => "L STICK",
            _ => b.ToString()
        };
    }

    static string MoveKeys(KeyboardMap k)
    {
        if (k.Forward == Keys.Up && k.Back == Keys.Down && k.Left == Keys.Left && k.Right == Keys.Right)
            return "ARROWS";
        string f = KeyName(k.Forward), l = KeyName(k.Left), b = KeyName(k.Back), r = KeyName(k.Right);
        return (f.Length == 1 && l.Length == 1 && b.Length == 1 && r.Length == 1)
            ? f + l + b + r                       // ex.: WASD
            : f + "/" + l + "/" + b + "/" + r;
    }

    public static string KeyName(Keys key) => key switch
    {
        Keys.LeftShift => "LSHIFT",
        Keys.RightShift => "RSHIFT",
        Keys.LeftControl => "LCTRL",
        Keys.RightControl => "RCTRL",
        Keys.LeftAlt => "LALT",
        Keys.RightAlt => "RALT",
        Keys.Space => "SPACE",
        Keys.Enter => "ENTER",
        Keys.Up => "UP",
        Keys.Down => "DOWN",
        Keys.Left => "LEFT",
        Keys.Right => "RIGHT",
        _ => key.ToString().ToUpperInvariant()
    };
}